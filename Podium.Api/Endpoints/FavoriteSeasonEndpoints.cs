namespace Podium.Api.Endpoints;

public static class FavoriteSeasonEndpoints
{
    public static void MapFavoriteSeasonEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/favorites")
            .WithTags("Favorites");

        // Get user's favorite seasons
        group.MapGet("/seasons", async (
            HttpContext context,
            [FromServices] IFavoriteSeasonRepository favoriteRepo) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var favorites = await favoriteRepo.GetUserFavoriteSeasonsAsync(userId);
            return Results.Ok(favorites);
        })
        .RequireAuth()
        .WithName("GetFavoriteSeasons");

        // Add a season to favorites
        group.MapPost("/seasons/{seasonId}", async (
            HttpContext context,
            string seasonId,
            [FromBody] AddFavoriteSeasonRequest request,
            [FromServices] IFavoriteSeasonRepository favoriteRepo,
            [FromServices] ISeasonRepository seasonRepo,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var count = await favoriteRepo.GetUserFavoriteCountAsync(userId);
            if (count >= 5)
                return Results.BadRequest(new { error = localizer["Favorites_LimitReached"].Value });

            var isFavorite = await favoriteRepo.IsFavoriteAsync(userId, seasonId);
            if (isFavorite)
                return Results.BadRequest(new { error = localizer["Favorites_AlreadyFavorited"].Value });

            var season = await seasonRepo.GetSeasonByIdOnlyAsync(seasonId);
            if (season == null)
                return Results.NotFound(new { error = localizer["Favorites_NotFound"].Value });

            var success = await favoriteRepo.AddFavoriteSeasonAsync(
                userId,
                seasonId,
                request.SeasonName,
                request.SeriesName,
                request.Year);

            if (!success)
                return Results.BadRequest(new { error = localizer["Favorites_AddFailed"].Value });

            return Results.Ok(new { message = localizer["Favorites_Added"].Value });
        })
        .RequireAuth()
        .WithName("AddFavoriteSeason");

        // Remove a season from favorites
        group.MapDelete("/seasons/{seasonId}", async (
            HttpContext context,
            string seasonId,
            [FromServices] IFavoriteSeasonRepository favoriteRepo,
            [FromServices] IStringLocalizer<ApiMessages> localizer) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var success = await favoriteRepo.RemoveFavoriteSeasonAsync(userId, seasonId);
            if (!success)
                return Results.BadRequest(new { error = localizer["Favorites_RemoveFailed"].Value });

            return Results.Ok(new { message = localizer["Favorites_Removed"].Value });
        })
        .RequireAuth()
        .WithName("RemoveFavoriteSeason");

        // Check if a season is favorited
        group.MapGet("/seasons/{seasonId}/check", async (
            HttpContext context,
            string seasonId,
            [FromServices] IFavoriteSeasonRepository favoriteRepo) =>
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var isFavorite = await favoriteRepo.IsFavoriteAsync(userId, seasonId);
            return Results.Ok(new { isFavorite });
        })
        .RequireAuth()
        .WithName("CheckFavoriteSeason");
    }
}

public record AddFavoriteSeasonRequest(
    string SeasonName,
    string SeriesName,
    int Year
);
