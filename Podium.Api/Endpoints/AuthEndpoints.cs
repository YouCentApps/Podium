namespace Podium.Api.Endpoints;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        // Send registration verification email
        group.MapPost("/register/send-verification", async (
            [FromBody] RegisterRequest request,
            [FromServices] IRegistrationService registrationService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var (success, tempUserId, errorMessage) = await registrationService.SendRegistrationVerificationAsync(
                request.Email,
                request.Username,
                request.Password,
                request.PreferredAuthMethod,
                request.LanguageCode ?? "en").ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { tempUserId, message = localizer["Auth_VerificationSent"].Value });
        })
        .WithName("SendRegistrationVerification");

        // Verify email and complete registration
        group.MapPost("/register/verify", async (
            [FromBody] VerifyRegistrationRequest request,
            [FromServices] IRegistrationService registrationService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var (success, userId, errorMessage) = await registrationService.VerifyAndCompleteRegistrationAsync(
                request.TempUserId,
                request.OtpCode).ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { userId, message = localizer["Auth_RegistrationSuccess"].Value });
        })
        .WithName("VerifyRegistration");

        // Register new user (direct - password-only without email)
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IRegistrationService registrationService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var (success, userId, errorMessage) = await registrationService.RegisterUserAsync(
                request.Email,
                request.Username,
                request.Password,
                request.PreferredAuthMethod,
                request.LanguageCode ?? "en").ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { userId, message = localizer["Auth_RegistrationSuccess"].Value });
        })
        .WithName("Register");

        // Send OTP to email
        group.MapPost("/send-otp", async (
            [FromBody] SendOtpRequest request,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var (success, actualEmail, errorMessage) = await authService.SendOTPAsync(request.EmailOrUsername).ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { message = localizer["Auth_OtpSent"].Value, email = actualEmail });
        })
        .WithName("SendOTP");

        // Verify OTP
        group.MapPost("/verify-otp", async (
            [FromBody] VerifyOtpRequest request,
            [FromServices] IAuthenticationService authService) =>
        {
            var (success, userId, username, sessionId, languageCode, errorMessage) = await authService.VerifyOTPAsync(
                request.Email,
                request.OtpCode).ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { userId, username, sessionId, languageCode });
        })
        .WithName("VerifyOTP");

        // Sign in with password
        group.MapPost("/signin", async (
            [FromBody] SignInRequest request,
            [FromServices] IAuthenticationService authService) =>
        {
            var (success, userId, username, sessionId, languageCode, errorMessage) = await authService.SignInWithPasswordAsync(
                request.EmailOrUsername,
                request.Password).ConfigureAwait(false);

            if (!success)
                return Results.BadRequest(new { error = errorMessage });

            return Results.Ok(new { userId, username, sessionId, languageCode });
        })
        .WithName("SignIn");

        // Validate session
        group.MapPost("/validate-session", async (
            [FromBody] ValidateSessionRequest request,
            [FromServices] IAuthenticationService authService) =>
        {
            var (success, userId, username, sessionId, languageCode, errorMessage) = 
                await authService.ValidateSessionAsync(request.SessionId).ConfigureAwait(false);

            if (!success)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { userId, username, sessionId, languageCode });
        })
        .WithName("ValidateSession");

        // Sign out
        group.MapPost("/signout", async (
            [FromBody] SignOutRequest request,
            [FromServices] IAuthenticationService authService,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            await authService.SignOutAsync(request.SessionId).ConfigureAwait(false);
            return Results.Ok(new { message = localizer["Auth_SignOutSuccess"].Value });
        })
        .WithName("SignOut");
    }
}

// Request DTOs
internal record RegisterRequest(string Email, string Username, string Password, string PreferredAuthMethod, string? LanguageCode);
internal record VerifyRegistrationRequest(string TempUserId, string OtpCode);
internal record SendOtpRequest(string EmailOrUsername);
internal record VerifyOtpRequest(string Email, string OtpCode);
internal record SignInRequest(string EmailOrUsername, string Password);
internal record ValidateSessionRequest(string SessionId);
internal record SignOutRequest(string SessionId);
