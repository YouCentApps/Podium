namespace Podium.Api.Middleware;

internal static class AuthorizationExtensions
{
    /// <summary>
    /// Validates session from X-Session-Id header and adds userId to request items
    /// </summary>
    public static async Task<IResult?> ValidateSession(
        HttpContext context,
        IAuthenticationService authService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authService);
        if (!context.Request.Headers.TryGetValue("X-Session-Id", out var sessionId) || 
            string.IsNullOrEmpty(sessionId))
        {
            return Results.Unauthorized();
        }

        var (success, userId, username, validSessionId, _, _) = 
            await authService.ValidateSessionAsync(sessionId!).ConfigureAwait(false);

        if (!success || string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Store userId and username in request items for use in endpoint handlers
        context.Items["UserId"] = userId;
        context.Items["Username"] = username;
        context.Items["SessionId"] = validSessionId;

        return null; // Success, continue to endpoint
    }

    /// <summary>
    /// Validates session and checks if user is an active admin
    /// </summary>
    public static async Task<IResult?> ValidateAdminSession(
        HttpContext context,
        IAuthenticationService authService,
        IAdminRepository adminRepo)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authService);
        ArgumentNullException.ThrowIfNull(adminRepo);
        var sessionResult = await ValidateSession(context, authService).ConfigureAwait(false);
        if (sessionResult != null)
        {
            return sessionResult; // Unauthorized
        }

        var userId = context.Items["UserId"] as string;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Check if user is an active admin
        var isActiveAdmin = await adminRepo.IsActiveAdminAsync(userId).ConfigureAwait(false);
        if (!isActiveAdmin)
        {
            return Results.Json(
                new { error = "Administrator privileges required" }, 
                statusCode: 403);
        }

        return null; // Success, continue to endpoint
    }

    /// <summary>
    /// Validates session and checks if user can manage admins
    /// </summary>
    public static async Task<IResult?> ValidateAdminManagementPermission(
        HttpContext context,
        IAuthenticationService authService,
        IAdminRepository adminRepo)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authService);
        ArgumentNullException.ThrowIfNull(adminRepo);
        var adminResult = await ValidateAdminSession(context, authService, adminRepo).ConfigureAwait(false);
        if (adminResult != null)
        {
            return adminResult;
        }

        var userId = context.Items["UserId"] as string;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // Check if user can manage admins
        var canManageAdmins = await adminRepo.CanManageAdminsAsync(userId).ConfigureAwait(false);
        if (!canManageAdmins)
        {
            return Results.Json(
                new { error = "Admin management privileges required" }, 
                statusCode: 403);
        }

        return null; // Success, continue to endpoint
    }

    /// <summary>
    /// Helper to get userId from HttpContext.Items (after ValidateSession)
    /// </summary>
    public static string? GetUserId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items["UserId"] as string;
    }

    /// <summary>
    /// Helper to get username from HttpContext.Items (after ValidateSession)
    /// </summary>
    public static string? GetUsername(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items["Username"] as string;
    }

    /// <summary>
    /// Helper to get sessionId from HttpContext.Items (after ValidateSession)
    /// </summary>
    public static string? GetSessionId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items["SessionId"] as string;
    }
}

/// <summary>
/// Route handler filter extensions for authentication and authorization
/// </summary>
internal static class EndpointAuthorizationExtensions
{
    /// <summary>
    /// Requires authenticated session. Adds filter that validates X-Session-Id header.
    /// </summary>
    public static RouteHandlerBuilder RequireAuth(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var authService = httpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            
            var result = await AuthorizationExtensions.ValidateSession(httpContext, authService).ConfigureAwait(false);
            if (result != null)
            {
                return result; // Unauthorized
            }

            return await next(context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Requires active admin privileges. Validates session and admin status.
    /// </summary>
    public static RouteHandlerBuilder RequireAdmin(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var authService = httpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            var adminRepo = httpContext.RequestServices.GetRequiredService<IAdminRepository>();
            
            var result = await AuthorizationExtensions.ValidateAdminSession(httpContext, authService, adminRepo).ConfigureAwait(false);
            if (result != null)
            {
                return result; // Unauthorized or Forbidden
            }

            return await next(context).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Requires admin management privileges. For creating/managing other admins.
    /// </summary>
    public static RouteHandlerBuilder RequireAdminManagement(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var authService = httpContext.RequestServices.GetRequiredService<IAuthenticationService>();
            var adminRepo = httpContext.RequestServices.GetRequiredService<IAdminRepository>();
            
            var result = await AuthorizationExtensions.ValidateAdminManagementPermission(httpContext, authService, adminRepo).ConfigureAwait(false);
            if (result != null)
            {
                return result; // Unauthorized or Forbidden
            }

            return await next(context).ConfigureAwait(false);
        });
    }
}
