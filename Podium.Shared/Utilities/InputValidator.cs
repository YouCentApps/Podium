using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;

namespace Podium.Shared.Utilities;

public static class InputValidator
{
    // Username validation: Latin letters, numbers, spaces - max 50 characters
    private static readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9 ]+$", RegexOptions.Compiled);
    
    // Password validation: Latin letters, numbers, common special chars - no spaces - max 100 characters
    private static readonly Regex PasswordRegex = new Regex(@"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$", RegexOptions.Compiled);
    
    // Email validation: basic email format
    private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static (bool IsValid, string? ErrorMessage) ValidateUsername(string username, IStringLocalizer<ApiMessages>? localizer = null)
    {
        var loc = localizer ?? new NullStringLocalizer<ApiMessages>();

        if (string.IsNullOrWhiteSpace(username))
        {
            return (false, loc["Val_UsernameRequired"].Value);
        }

        var trimmed = username.Trim();

        if (trimmed.Length < 3)
        {
            return (false, loc["Val_UsernameTooShort"].Value);
        }

        if (trimmed.Length > 50)
        {
            return (false, loc["Val_UsernameTooLong"].Value);
        }

        if (!UsernameRegex.IsMatch(trimmed))
        {
            return (false, loc["Val_UsernameChars"].Value);
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidatePassword(string password, IStringLocalizer<ApiMessages>? localizer = null)
    {
        var loc = localizer ?? new NullStringLocalizer<ApiMessages>();

        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, loc["Val_PasswordRequired"].Value);
        }

        if (password.Length < 6)
        {
            return (false, loc["Val_PasswordTooShort"].Value);
        }

        if (password.Length > 100)
        {
            return (false, loc["Val_PasswordTooLong"].Value);
        }

        if (password.Contains(' ', StringComparison.Ordinal))
        {
            return (false, loc["Val_PasswordNoSpaces"].Value);
        }

        if (!PasswordRegex.IsMatch(password))
        {
            return (false, loc["Val_PasswordChars"].Value);
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateEmail(string email, IStringLocalizer<ApiMessages>? localizer = null)
    {
        var loc = localizer ?? new NullStringLocalizer<ApiMessages>();

        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, loc["Val_EmailRequired"].Value);
        }

        var trimmed = email.Trim();

        if (!EmailRegex.IsMatch(trimmed))
        {
            return (false, loc["Val_EmailInvalid"].Value);
        }

        return (true, null);
    }

    public static string NormalizeUsername(string username)
    {
        // Normalize username for lookup: lowercase and trim
        return username.Trim().ToLowerInvariant();
    }
}
