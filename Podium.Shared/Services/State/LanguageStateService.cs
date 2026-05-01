using System.Globalization;

namespace Podium.Shared.Services.State;

public class LanguageStateService
{
    private const string StorageKey = "podium_language";

    private readonly IStorageService? _storageService;

    public event Action? OnLanguageChanged;

    public string CurrentLanguageCode { get; private set; } = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    public LanguageStateService(IStorageService? storageService = null)
    {
        _storageService = storageService;
    }

    public async Task InitializeAsync()
    {
        if (_storageService == null)
            return;

        var stored = await _storageService.GetItemAsync<string>(StorageKey).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(stored))
            CurrentLanguageCode = stored;
    }

    /// <summary>
    /// Persists the language choice locally. On non-browser platforms (MAUI)
    /// the culture is applied immediately so IStringLocalizer picks it up
    /// without a page reload.
    /// </summary>
    public async Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguageCode = languageCode;

        if (_storageService != null)
            await _storageService.SetItemAsync(StorageKey, languageCode).ConfigureAwait(false);

        // On MAUI there is no page reload, so apply the culture directly
        if (!OperatingSystem.IsBrowser())
        {
            var culture = new CultureInfo(languageCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        OnLanguageChanged?.Invoke();
    }
}
