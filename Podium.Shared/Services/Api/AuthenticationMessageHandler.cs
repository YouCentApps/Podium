using System.Net;
using Podium.Shared.Services.State;

namespace Podium.Shared.Services.Api;

/// <summary>
/// HTTP message handler that automatically injects the X-Session-Id and Accept-Language
/// headers from state services into all API requests, and handles session expiration.
/// </summary>
public class AuthenticationMessageHandler(AuthStateService authStateService, LanguageStateService languageStateService) : DelegatingHandler
{
    private readonly AuthStateService _authStateService = authStateService;
    private readonly LanguageStateService _languageStateService = languageStateService;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var isAuthEndpoint = request.RequestUri?.PathAndQuery.Contains("/api/auth/", StringComparison.Ordinal) ?? false;

        if (!isAuthEndpoint && !string.IsNullOrEmpty(_authStateService.SessionId))
        {
            request.Headers.Add("X-Session-Id", _authStateService.SessionId);
        }

        // Always send Accept-Language so the API responds in the user's language
        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.ParseAdd(_languageStateService.CurrentLanguageCode);

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthEndpoint)
        {
            await _authStateService.HandleSessionExpiredAsync().ConfigureAwait(false);
        }

        return response;
    }
}
