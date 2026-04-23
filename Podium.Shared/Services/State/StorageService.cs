using Microsoft.JSInterop;
using System.Text.Json;

namespace Podium.Shared.Services.State;

/// <summary>
/// Service for persisting data to browser localStorage (Web) or device storage (MAUI)
/// </summary>
public interface IStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}

/// <summary>
/// Browser localStorage implementation for Blazor WebAssembly
/// </summary>
public class BrowserStorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var raw = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key).ConfigureAwait(false);
            if (string.IsNullOrEmpty(raw))
                return default;

            // Strings are stored raw (no JSON wrapper) for JS interop compatibility
            if (typeof(T) == typeof(string))
                return (T?)(object?)raw;

            return JsonSerializer.Deserialize<T>(raw);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            // Strings are stored raw so plain JS (podiumCulture.get/set) can read them
            var data = value is string str ? str : JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, data).ConfigureAwait(false);
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key).ConfigureAwait(false);
        }
        catch
        {
            // Silently fail
        }
    }
}

/// <summary>
/// Session data model for persistence
/// </summary>
public class SessionData
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}
