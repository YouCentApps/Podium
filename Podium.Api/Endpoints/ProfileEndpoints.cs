using Podium.Shared.Utilities;

namespace Podium.Api.Endpoints;

internal static class ProfileEndpoints
{
    private static readonly string[] ValidAuthMethods = ["Email", "Password", "Both"];

    public static void MapProfileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("Profile");

        // Get current user's profile
        group.MapGet("/", async (
            HttpContext context,
            [FromServices] IUserRepository userRepository,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            return Results.Ok(new UserProfileResponse(
                user.UserId,
                user.Email,
                user.Username,
                user.PreferredAuthMethod,
                HasPassword: !string.IsNullOrEmpty(user.PasswordHash),
                HasEmail: !string.IsNullOrEmpty(user.Email)
            ));
        })
        .RequireAuth()
        .WithName("GetProfile");

        // Update username
        group.MapPost("/username", async (
            HttpContext context,
            [FromBody] UpdateUsernameRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            var verified = await VerifyIdentityAsync(user, request.Password, request.OtpCode, authService, localizer).ConfigureAwait(false);
            if (!verified.Success)
                return Results.BadRequest(new { error = verified.Error });

            var (usernameValid, _) = InputValidator.ValidateUsername(request.NewUsername);
            if (!usernameValid)
                return Results.BadRequest(new { error = localizer["Val_UsernameInvalid"].Value });

            var existingUser = await userRepository.GetUserByUsernameAsync(request.NewUsername).ConfigureAwait(false);
            if (existingUser != null && existingUser.UserId != userId)
                return Results.BadRequest(new { error = localizer["Profile_UsernameTaken"].Value });

            user.Username = request.NewUsername;
            user.NormalizedUsername = InputValidator.NormalizeUsername(request.NewUsername);
            var success = await userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error = localizer["Profile_UpdateFailed"].Value });

            return Results.Ok(new { message = localizer["Profile_UsernameUpdated"].Value });
        })
        .RequireAuth()
        .WithName("UpdateUsername");

        // Update auth method
        group.MapPost("/auth-method", async (
            HttpContext context,
            [FromBody] UpdateAuthMethodRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            if (!ValidAuthMethods.Contains(request.NewAuthMethod))
                return Results.BadRequest(new { error = localizer["Profile_InvalidAuthMethod"].Value });

            var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            var hasEmail = !string.IsNullOrEmpty(user.Email);

            if ((request.NewAuthMethod == "Password" || request.NewAuthMethod == "Both") && !hasPassword)
            {
                return Results.BadRequest(new {
                    error = localizer["Profile_NeedPasswordFirst"].Value,
                    requiresPasswordSetup = true
                });
            }

            if ((request.NewAuthMethod == "Email" || request.NewAuthMethod == "Both") && !hasEmail)
            {
                return Results.BadRequest(new {
                    error = localizer["Profile_NeedEmailFirst"].Value,
                    requiresEmailSetup = true
                });
            }

            var verified = await VerifyIdentityAsync(user, request.Password, request.OtpCode, authService, localizer).ConfigureAwait(false);
            if (!verified.Success)
                return Results.BadRequest(new { error = verified.Error });

            user.PreferredAuthMethod = request.NewAuthMethod;
            var success = await userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error = localizer["Profile_UpdateFailed"].Value });

            return Results.Ok(new { message = localizer["Profile_AuthMethodUpdated"].Value });
        })
        .RequireAuth()
        .WithName("UpdateAuthMethod");

        // Update password
        group.MapPost("/password", async (
            HttpContext context,
            [FromBody] UpdatePasswordRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                return Results.BadRequest(new { error = localizer["Val_PasswordInvalid"].Value });

            var hasExistingPassword = !string.IsNullOrEmpty(user.PasswordHash);

            if (hasExistingPassword)
            {
                if (string.IsNullOrEmpty(request.OldPassword))
                    return Results.BadRequest(new { error = localizer["Profile_OldPasswordRequired"].Value });

                var verified = await VerifyIdentityAsync(user, request.OldPassword, null, authService, localizer).ConfigureAwait(false);
                if (!verified.Success)
                    return Results.BadRequest(new { error = localizer["Profile_OldPasswordIncorrect"].Value });
            }
            else
            {
                if (string.IsNullOrEmpty(request.OtpCode))
                    return Results.BadRequest(new { error = localizer["Profile_OtpRequired"].Value });

                var verified = await VerifyIdentityAsync(user, null, request.OtpCode, authService, localizer).ConfigureAwait(false);
                if (!verified.Success)
                    return Results.BadRequest(new { error = verified.Error });
            }

            var (hash, salt) = AuthenticationService.HashPassword(request.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            var success = await userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error = localizer["Profile_UpdateFailed"].Value });

            return Results.Ok(new { message = localizer["Profile_PasswordUpdated"].Value });
        })
        .RequireAuth()
        .WithName("UpdatePassword");

        // Send OTP for password setup (when user doesn't have password)
        group.MapPost("/password/send-otp", async (
            HttpContext context,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            if (string.IsNullOrEmpty(user.Email))
                return Results.BadRequest(new { error = localizer["Profile_NoEmailForSetup"].Value });

            var (success, _, error) = await authService.SendOTPAsync(user.Email).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error });

            return Results.Ok(new { message = localizer["Profile_PasswordSetupSent"].Value });
        })
        .RequireAuth()
        .WithName("SendPasswordSetupOtp");

        // Send OTP for email update
        group.MapPost("/email/send-otp", async (
            HttpContext context,
            [FromBody] SendEmailUpdateOtpRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.NewEmail) || !request.NewEmail.Contains('@', StringComparison.Ordinal))
                return Results.BadRequest(new { error = localizer["Val_EmailInvalid"].Value });

            var existingUser = await userRepository.GetUserByEmailAsync(request.NewEmail).ConfigureAwait(false);
            if (existingUser != null && existingUser.UserId != userId)
                return Results.BadRequest(new { error = localizer["Reg_EmailTaken"].Value });

            var (success, error) = await authService.SendOTPForNewEmailAsync(request.NewEmail, userId).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error });

            return Results.Ok(new { message = localizer["Profile_VerificationSent"].Value });
        })
        .RequireAuth()
        .WithName("SendEmailUpdateOtp");

        // Confirm email update with OTP
        group.MapPost("/email/confirm", async (
            HttpContext context,
            [FromBody] ConfirmEmailUpdateRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            if (string.IsNullOrWhiteSpace(request.NewEmail) || !request.NewEmail.Contains('@', StringComparison.Ordinal))
                return Results.BadRequest(new { error = localizer["Val_EmailInvalid"].Value });

            if (string.IsNullOrEmpty(request.OtpCode))
                return Results.BadRequest(new { error = localizer["Auth_InvalidOtp"].Value });

            var (success, error) = await authService.VerifyOTPCodeAsync(request.NewEmail, request.OtpCode).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error = localizer["Auth_InvalidOtp"].Value });

#pragma warning disable CA1308
            user.Email = request.NewEmail.ToLowerInvariant();
#pragma warning restore CA1308
            var updateSuccess = await userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            if (!updateSuccess)
                return Results.BadRequest(new { error = localizer["Profile_UpdateFailed"].Value });

            return Results.Ok(new { message = localizer["Profile_EmailUpdated"].Value });
        })
        .RequireAuth()
        .WithName("ConfirmEmailUpdate");

        // Update language preference
        group.MapPost("/language", async (
            HttpContext context,
            [FromBody] UpdateLanguageRequest request,
            [FromServices] IUserRepository userRepository,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            if (!Podium.Shared.Models.SupportedLanguages.All.Any(l => l.Code == request.LanguageCode))
                return Results.BadRequest(new { error = localizer["Profile_InvalidLanguage"].Value });

            user.LanguageCode = request.LanguageCode;
            var success = await userRepository.UpdateUserAsync(user).ConfigureAwait(false);
            if (!success)
                return Results.BadRequest(new { error = localizer["Profile_UpdateFailed"].Value });

            return Results.Ok(new { message = localizer["Profile_LanguageUpdated"].Value });
        })
        .RequireAuth()
        .WithName("UpdateLanguage");

        // Get user's language preference from DB
        group.MapGet("/language", async (
            HttpContext context,
            [FromServices] IUserRepository userRepository,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userRepository.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Results.NotFound(new { error = localizer["Profile_NotFound"].Value });

            return Results.Ok(new { languageCode = user.LanguageCode });
        })
        .RequireAuth()
        .WithName("GetLanguage");

        // Get supported languages (static list, no auth required)
        group.MapGet("/languages", () =>
        {
            var languages = Podium.Shared.Models.SupportedLanguages.All
                .Select(l => new { l.Code, l.Name });
            return Results.Ok(languages);
        })
        .WithName("GetLanguages")
        .AllowAnonymous();
    }

    private static async Task<(bool Success, string Error)> VerifyIdentityAsync(
        User user,
        string? password,
        string? otpCode,
        IAuthenticationService authService,
        IStringLocalizer<ApiMessages> localizer)
    {
        var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
        var hasEmail = !string.IsNullOrEmpty(user.Email);

        var passwordAuthEnabled = user.PreferredAuthMethod == "Password" || user.PreferredAuthMethod == "Both";
        var emailAuthEnabled = user.PreferredAuthMethod == "Email" || user.PreferredAuthMethod == "Both";

        if (!string.IsNullOrEmpty(password))
        {
            if (hasPassword && passwordAuthEnabled)
            {
                var (success, _) = await authService.VerifyPasswordAsync(user.Email, password).ConfigureAwait(false);
                if (success)
                    return (true, string.Empty);
                return (false, localizer["Auth_InvalidCredentials"].Value);
            }
            else if (hasPassword && !passwordAuthEnabled)
            {
                return (false, localizer["Profile_PasswordAuthDisabled"].Value);
            }
            else
            {
                return (false, localizer["Profile_NoPasswordSet"].Value);
            }
        }

        if (!string.IsNullOrEmpty(otpCode))
        {
            if (hasEmail && emailAuthEnabled)
            {
                var (success, _) = await authService.VerifyOTPCodeAsync(user.Email, otpCode).ConfigureAwait(false);
                if (success)
                    return (true, string.Empty);
                return (false, localizer["Auth_InvalidOtp"].Value);
            }
            else if (hasEmail && !emailAuthEnabled)
            {
                return (false, localizer["Profile_EmailAuthDisabled"].Value);
            }
            else
            {
                return (false, localizer["Profile_NoEmailSet"].Value);
            }
        }

        if (passwordAuthEnabled && emailAuthEnabled)
            return (false, localizer["Profile_ProvidePasswordOrCode"].Value);
        else if (passwordAuthEnabled)
            return (false, localizer["Profile_ProvidePassword"].Value);
        else if (emailAuthEnabled)
            return (false, localizer["Profile_ProvideCode"].Value);
        else
            return (false, localizer["Profile_IdentityVerificationFailed"].Value);
    }
}

// Request DTOs
public record UpdateUsernameRequest(string NewUsername, string? Password, string? OtpCode);
public record UpdateAuthMethodRequest(string NewAuthMethod, string? Password, string? OtpCode);
public record UpdatePasswordRequest(string? OldPassword, string? OtpCode, string NewPassword);
public record SendEmailUpdateOtpRequest(string NewEmail);
public record ConfirmEmailUpdateRequest(string NewEmail, string OtpCode);
public record UpdateLanguageRequest(string LanguageCode);
public record UserProfileResponse(
    string UserId,
    string Email,
    string Username,
    string PreferredAuthMethod,
    bool HasPassword,
    bool HasEmail);
