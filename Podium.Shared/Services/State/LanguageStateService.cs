using Microsoft.JSInterop;

namespace Podium.Shared.Services.State;

public class LanguageStateService
{
    private const string StorageKey = "podium_language";
    private const string DefaultLanguage = "en";

    private readonly IStorageService? _storageService;

    public event Action? OnLanguageChanged;

    public string CurrentLanguageCode { get; private set; } = DefaultLanguage;

    public LanguageStateService(IStorageService? storageService = null)
    {
        _storageService = storageService;
    }

    public async Task InitializeAsync()
    {
        if (_storageService == null)
            return;

        var stored = await _storageService.GetItemAsync<string>(StorageKey);
        if (!string.IsNullOrEmpty(stored))
            CurrentLanguageCode = stored;
    }

    /// <summary>
    /// Persists the language choice locally. The caller is responsible for
    /// triggering the culture change (page reload on WASM, direct set on MAUI).
    /// </summary>
    public async Task SetLanguageAsync(string languageCode)
    {
        CurrentLanguageCode = languageCode;

        if (_storageService != null)
            await _storageService.SetItemAsync(StorageKey, languageCode);

        OnLanguageChanged?.Invoke();
    }
}
