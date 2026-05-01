using Microsoft.Extensions.Localization;

namespace Podium.Shared.Utilities;

/// <summary>
/// A no-op IStringLocalizer that returns the key as-is.
/// Used as a fallback when no localizer is injected (e.g., in unit tests or
/// when a service is constructed without DI).
/// </summary>
public class NullStringLocalizer<T> : IStringLocalizer<T>
{
    public LocalizedString this[string name] => new LocalizedString(name, name, resourceNotFound: true);

    public LocalizedString this[string name, params object[] arguments]
        => new LocalizedString(name, string.Format(System.Globalization.CultureInfo.InvariantCulture, name, arguments), resourceNotFound: true);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        => Enumerable.Empty<LocalizedString>();
}
