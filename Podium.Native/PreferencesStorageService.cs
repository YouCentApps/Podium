using Podium.Shared.Services.State;
using System.Text.Json;

namespace Podium.Native;

/// <summary>
/// MAUI Preferences-backed implementation of <see cref="IStorageService"/>.
/// Uses <see cref="Microsoft.Maui.Storage.Preferences"/> so the stored values
/// are consistent with the startup bootstrap in <see cref="MauiProgram"/>.
/// </summary>
public class PreferencesStorageService : IStorageService
{
    public Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            var raw = Preferences.Default.Get<string?>(key, null);
            if (string.IsNullOrEmpty(raw))
                return Task.FromResult(default(T?));

            if (typeof(T) == typeof(string))
                return Task.FromResult((T?)(object?)raw);

            return Task.FromResult(JsonSerializer.Deserialize<T>(raw));
        }
        catch
        {
            return Task.FromResult(default(T?));
        }
    }

    public Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var data = value is string str ? str : JsonSerializer.Serialize(value);
            Preferences.Default.Set(key, data);
        }
        catch
        {
            // Silently fail if Preferences is not available
        }

        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string key)
    {
        try
        {
            Preferences.Default.Remove(key);
        }
        catch
        {
            // Silently fail
        }

        return Task.CompletedTask;
    }
}
