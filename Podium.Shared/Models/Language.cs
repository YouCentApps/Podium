namespace Podium.Shared.Models;

public class Language
{
    public string Code { get; set; } = string.Empty;   // "en", "fr", "ru"
    public string Name { get; set; } = string.Empty;   // "English", "Français", "Русский"
    public bool IsActive { get; set; }
}

/// <summary>
/// Single source of truth for supported languages.
/// Used by both the API (validation + GET /languages) and as the client-side fallback.
/// </summary>
public static class SupportedLanguages
{
    public static readonly Language[] All =
    [
        new() { Code = "en", Name = "English", IsActive = true },
        new() { Code = "fr", Name = "Français", IsActive = true },
        new() { Code = "es", Name = "Español", IsActive = true },
        new() { Code = "de", Name = "Deutsch", IsActive = true },
        new() { Code = "uk", Name = "Українська", IsActive = true },
        new() { Code = "ru", Name = "Русский", IsActive = true },
        new() { Code = "it", Name = "Italiano", IsActive = true },
        new() { Code = "nb", Name = "Norsk", IsActive = true }
    ];
}
