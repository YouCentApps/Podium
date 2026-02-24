namespace Podium.Shared.Models;

public class Language
{
    public string Code { get; set; } = string.Empty;   // "en", "fr", "ru"
    public string Name { get; set; } = string.Empty;   // "English", "Français", "Русский"
    public bool IsActive { get; set; }
}
