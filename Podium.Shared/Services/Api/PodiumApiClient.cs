using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Podium.Shared.Utilities;

namespace Podium.Shared.Services.Api;

public interface IPodiumApiClient
{
    // Authentication
    Task<ApiResponse<RegisterVerificationResponse>> SendRegistrationVerificationAsync(string email, string username, string password, string preferredAuthMethod, string languageCode);
    Task<ApiResponse<RegisterResponse>> VerifyRegistrationAsync(string tempUserId, string otpCode);
    Task<ApiResponse<RegisterResponse>> RegisterAsync(string email, string username, string password, string preferredAuthMethod, string languageCode);
    Task<ApiResponse<SendOtpResponse>> SendOtpAsync(string emailOrUsername);
    Task<ApiResponse<AuthResponse>> VerifyOtpAsync(string email, string otpCode);
    Task<ApiResponse<AuthResponse>> SignInAsync(string emailOrUsername, string password);
    Task<ApiResponse<AuthResponse>> ValidateSessionAsync(string sessionId);
    Task<ApiResponse<MessageResponse>> SignOutAsync(string sessionId);

    // Disciplines & Competitions
    Task<ApiResponse<List<Discipline>>> GetDisciplinesAsync();
    Task<ApiResponse<List<Series>>> GetSeriesAsync(string disciplineId);
    Task<ApiResponse<List<Season>>> GetSeasonsAsync(string seriesId);
    Task<ApiResponse<Season>> GetActiveSeasonAsync(string seriesId);
    Task<ApiResponse<List<Event>>> GetEventsAsync(string seasonId);
    Task<ApiResponse<List<Event>>> GetUpcomingEventsAsync(string seasonId);
    Task<ApiResponse<List<SeasonCompetitor>>> GetCompetitorsAsync(string seasonId);
    Task<ApiResponse<Event>> GetEventDetailsAsync(string eventId, string seasonId);
    Task<ApiResponse<EventResult>> GetEventResultAsync(string eventId);

    // Predictions
    Task<ApiResponse<Prediction>> GetPredictionAsync(string eventId, string userId);
    Task<ApiResponse<List<Prediction>>> GetEventPredictionsAsync(string eventId);
    Task<ApiResponse<List<Prediction>>> GetUserSeasonPredictionsAsync(string userId, string seasonId);
    Task<ApiResponse<List<Prediction>>> GetUserActiveSeasonsPredictionsAsync(string userId);
    Task<ApiResponse<List<PredictionWithDetails>>> GetUserPredictionsWithDetailsAsync(string userId, string? seasonId = null, string? seriesId = null, string? disciplineId = null, bool includeInactive = false);
    Task<ApiResponse<PredictionResponse>> SubmitPredictionAsync(SubmitPredictionRequest request);
    Task<ApiResponse<LatestScoredPredictionResponse>> GetLatestScoredPredictionFromActiveAsync(string userId);

    // Leaderboard
    Task<ApiResponse<List<UserStatistics>>> GetLeaderboardAsync(string seasonId);
    Task<ApiResponse<UserStatistics>> GetUserStatisticsAsync(string seasonId, string userId);
    Task<ApiResponse<EventResultDetails>> GetLastEventResultAsync(string seasonId);
    Task<ApiResponse<List<UserEventPrediction>>> SearchLastEventResultAsync(string seasonId, string query);

    // Admin - Season Management
    Task<ApiResponse<MessageResponse>> SetActiveSeasonAsync(string seriesId, string seasonId);
    Task<ApiResponse<MessageResponse>> FindDuplicateActiveSeasonsAsync();

    // Admin - Admin Management
    Task<ApiResponse<AdminStatusResponse>> GetMyAdminStatusAsync();
    Task<ApiResponse<List<Admin>>> GetAllAdminsAsync();
    Task<ApiResponse<Admin>> GetAdminAsync(string userId);
    Task<ApiResponse<MessageResponse>> CreateAdminAsync(string userId, bool isActive, bool canManageAdmins);
    Task<ApiResponse<MessageResponse>> UpdateAdminAsync(string userId, bool isActive, bool canManageAdmins);
    Task<ApiResponse<MessageResponse>> RemoveAdminAsync(string userId);

    // Admin - Discipline Management
    Task<ApiResponse<List<Discipline>>> GetAllDisciplinesAdminAsync();
    Task<ApiResponse<Discipline>> GetDisciplineAdminAsync(string disciplineId);
    Task<ApiResponse<Discipline>> CreateDisciplineAsync(CreateDisciplineRequest request);
    Task<ApiResponse<Discipline>> UpdateDisciplineAsync(string disciplineId, UpdateDisciplineRequest request);
    Task<ApiResponse<MessageResponse>> DeleteDisciplineAsync(string disciplineId);

    // Admin - Series Management
    Task<ApiResponse<List<Series>>> GetAllSeriesAdminAsync(string disciplineId);
    Task<ApiResponse<Series>> GetSeriesAdminAsync(string seriesId);
    Task<ApiResponse<Series>> CreateSeriesAsync(CreateSeriesRequest request);
    Task<ApiResponse<Series>> UpdateSeriesAsync(string seriesId, string currentDisciplineId, UpdateSeriesRequest request);
    Task<ApiResponse<MessageResponse>> DeleteSeriesAsync(string seriesId, string disciplineId);

    // Admin - Season Management
    Task<ApiResponse<List<Season>>> GetAllSeasonsAdminAsync(string seriesId);
    Task<ApiResponse<Season>> GetSeasonAdminAsync(string seasonId, string seriesId);
    Task<ApiResponse<SeasonDependencies>> GetSeasonDependenciesAsync(string seasonId);
    Task<ApiResponse<Season>> CreateSeasonAsync(CreateSeasonRequest request);
    Task<ApiResponse<Season>> UpdateSeasonAsync(string seasonId, string currentSeriesId, UpdateSeasonRequest request);
    Task<ApiResponse<MessageResponse>> DeleteSeasonAsync(string seasonId, string seriesId);

    // Admin - Competitor Management
    Task<ApiResponse<List<Competitor>>> GetAllCompetitorsAdminAsync();
    Task<ApiResponse<List<Competitor>>> GetCompetitorsByTypeAsync(string type);
    Task<ApiResponse<Competitor>> GetCompetitorAdminAsync(string competitorId);
    Task<ApiResponse<CompetitorDependencies>> GetCompetitorDependenciesAsync(string competitorId);
    Task<ApiResponse<List<string>>> GetCompetitorSeasonsAsync(string competitorId);
    Task<ApiResponse<Competitor>> CreateCompetitorAsync(CreateCompetitorRequest request);
    Task<ApiResponse<Competitor>> UpdateCompetitorAsync(string competitorId, UpdateCompetitorRequest request);
    Task<ApiResponse<MessageResponse>> DeleteCompetitorAsync(string competitorId, string type);

    // Admin - Season Competitor Management
    Task<ApiResponse<List<SeasonCompetitor>>> GetSeasonCompetitorsAdminAsync(string seasonId);
    Task<ApiResponse<MessageResponse>> AddCompetitorToSeasonAsync(string seasonId, string competitorId);
    Task<ApiResponse<MessageResponse>> RemoveCompetitorFromSeasonAsync(string seasonId, string competitorId);

    // Admin - Event Management
    Task<ApiResponse<List<Event>>> GetAllEventsAsync();
    Task<ApiResponse<List<Event>>> GetEventsBySeasonAdminAsync(string seasonId);
    Task<ApiResponse<Event>> GetEventAdminAsync(string eventId);
    Task<ApiResponse<EventDependencies>> GetEventDependenciesAsync(string eventId);
    Task<ApiResponse<object>> GetNextEventNumberAsync(string seasonId);
    Task<ApiResponse<Event>> CreateEventAsync(CreateEventRequest request);
    Task<ApiResponse<Event>> UpdateEventAsync(string eventId, UpdateEventRequest request);
    Task<ApiResponse<MessageResponse>> DeleteEventAsync(string eventId, string seasonId);

    // Admin - Event Result Management
    Task<ApiResponse<EventResult>> GetEventResultAdminAsync(string eventId);
    Task<ApiResponse<EventResult>> CreateOrUpdateEventResultAsync(string eventId, CreateEventResultRequest request);
    Task<ApiResponse<MessageResponse>> DeleteEventResultAsync(string eventId);

    // Admin - Scoring & Predictions
    Task<ApiResponse<ScoringRules>> GetScoringRulesAsync(string seasonId);
    Task<ApiResponse<MessageResponse>> UpdatePredictionPointsAsync(string eventId, string userId, int points);

    // Admin - Statistics Recalculation
    Task<ApiResponse<RecalculationJobResponse>> StartStatisticsRecalculationAsync(string seasonId);
    Task<ApiResponse<JobStatusResponse>> GetStatisticsJobStatusAsync(string jobId);
    Task<ApiResponse<StatisticsUpdateVerification>> VerifyStatisticsUpdatedAsync(string seasonId, DateTime afterTimestamp);

    // Admin - User Management
    Task<ApiResponse<List<User>>> GetAllUsersAsync();
    Task<ApiResponse<List<UserSearchResult>>> SearchUsersAsync(string searchTerm);
    Task<ApiResponse<User>> GetUserAdminAsync(string userId);
    Task<ApiResponse<UserDependencies>> GetUserDependenciesAsync(string userId);
    Task<ApiResponse<MessageResponse>> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<ApiResponse<MessageResponse>> DeleteUserAsync(string userId);

    // Profile Management
    Task<ApiResponse<UserProfileResponse>> GetMyProfileAsync();
    Task<ApiResponse<MessageResponse>> UpdateUsernameAsync(UpdateUsernameRequest request);
    Task<ApiResponse<MessageResponse>> UpdateAuthMethodAsync(UpdateAuthMethodRequest request);
    Task<ApiResponse<MessageResponse>> UpdatePasswordAsync(UpdatePasswordRequest request);
    Task<ApiResponse<MessageResponse>> SendEmailUpdateOtpAsync(string newEmail);
    Task<ApiResponse<MessageResponse>> ConfirmEmailUpdateAsync(ConfirmEmailUpdateRequest request);
    Task<ApiResponse<MessageResponse>> SendPasswordSetupOtpAsync();

    // Language
    Task<ApiResponse<List<Language>>> GetLanguagesAsync();
    Task<ApiResponse<MessageResponse>> UpdateMyLanguageAsync(string languageCode);
    Task<ApiResponse<UserLanguageResponse>> GetMyLanguageAsync();

    // Favorite Seasons
    Task<ApiResponse<List<FavoriteSeason>>> GetFavoriteSeasonsAsync();
    Task<ApiResponse<MessageResponse>> AddFavoriteSeasonAsync(string seasonId, string seasonName, string seriesName, int year);
    Task<ApiResponse<MessageResponse>> RemoveFavoriteSeasonAsync(string seasonId);
    Task<ApiResponse<CheckFavoriteResponse>> CheckFavoriteSeasonAsync(string seasonId);
}

public class PodiumApiClient(HttpClient httpClient, IStringLocalizer<ApiMessages>? localizer = null) : IPodiumApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IStringLocalizer<ApiMessages> _localizer = localizer ?? new NullStringLocalizer<ApiMessages>();

    // Authentication
    public async Task<ApiResponse<RegisterVerificationResponse>> SendRegistrationVerificationAsync(string email, string username, string password, string preferredAuthMethod, string languageCode)
    {
        return await PostAsync<RegisterVerificationResponse>("/api/auth/register/send-verification", 
            new { email, username, password, preferredAuthMethod, languageCode }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<RegisterResponse>> VerifyRegistrationAsync(string tempUserId, string otpCode)
    {
        return await PostAsync<RegisterResponse>("/api/auth/register/verify", 
            new { tempUserId, otpCode }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<RegisterResponse>> RegisterAsync(string email, string username, string password, string preferredAuthMethod, string languageCode)
    {
        return await PostAsync<RegisterResponse>("/api/auth/register", 
            new { email, username, password, preferredAuthMethod, languageCode }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<SendOtpResponse>> SendOtpAsync(string emailOrUsername)
    {
        return await PostAsync<SendOtpResponse>("/api/auth/send-otp", new { emailOrUsername }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<AuthResponse>> VerifyOtpAsync(string email, string otpCode)
    {
        return await PostAsync<AuthResponse>("/api/auth/verify-otp", new { email, otpCode }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<AuthResponse>> SignInAsync(string emailOrUsername, string password)
    {
        return await PostAsync<AuthResponse>("/api/auth/signin", new { emailOrUsername, password }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<AuthResponse>> ValidateSessionAsync(string sessionId)
    {
        return await PostAsync<AuthResponse>("/api/auth/validate-session", new { sessionId }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> SignOutAsync(string sessionId)
    {
        return await PostAsync<MessageResponse>("/api/auth/signout", new { sessionId }).ConfigureAwait(false);
    }

    // Disciplines & Competitions
    public async Task<ApiResponse<List<Discipline>>> GetDisciplinesAsync()
    {
        return await GetAsync<List<Discipline>>("/api/disciplines").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Series>>> GetSeriesAsync(string disciplineId)
    {
        return await GetAsync<List<Series>>($"/api/disciplines/{disciplineId}/series").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Season>>> GetSeasonsAsync(string seriesId)
    {
        return await GetAsync<List<Season>>($"/api/series/{seriesId}/seasons").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Season>> GetActiveSeasonAsync(string seriesId)
    {
        return await GetAsync<Season>($"/api/series/{seriesId}/seasons/active").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Event>>> GetEventsAsync(string seasonId)
    {
        return await GetAsync<List<Event>>($"/api/seasons/{seasonId}/events").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Event>>> GetUpcomingEventsAsync(string seasonId)
    {
        return await GetAsync<List<Event>>($"/api/seasons/{seasonId}/events/upcoming").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<SeasonCompetitor>>> GetCompetitorsAsync(string seasonId)
    {
        return await GetAsync<List<SeasonCompetitor>>($"/api/seasons/{seasonId}/competitors").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Event>> GetEventDetailsAsync(string eventId, string seasonId)
    {
        return await GetAsync<Event>($"/api/events/{eventId}?seasonId={seasonId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<EventResult>> GetEventResultAsync(string eventId)
    {
        return await GetAsync<EventResult>($"/api/events/{eventId}/result").ConfigureAwait(false);
    }

    // Predictions
    public async Task<ApiResponse<Prediction>> GetPredictionAsync(string eventId, string userId)
    {
        return await GetAsync<Prediction>($"/api/predictions/{eventId}/user/{userId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Prediction>>> GetEventPredictionsAsync(string eventId)
    {
        return await GetAsync<List<Prediction>>($"/api/predictions/{eventId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Prediction>>> GetUserSeasonPredictionsAsync(string userId, string seasonId)
    {
        return await GetAsync<List<Prediction>>($"/api/predictions/user/{userId}/season/{seasonId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Prediction>>> GetUserActiveSeasonsPredictionsAsync(string userId)
    {
        return await GetAsync<List<Prediction>>($"/api/predictions/user/{userId}/active-seasons").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<PredictionWithDetails>>> GetUserPredictionsWithDetailsAsync(string userId, string? seasonId = null, string? seriesId = null, string? disciplineId = null, bool includeInactive = false)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(seasonId)) queryParams.Add($"seasonId={seasonId}");
        if (!string.IsNullOrEmpty(seriesId)) queryParams.Add($"seriesId={seriesId}");
        if (!string.IsNullOrEmpty(disciplineId)) queryParams.Add($"disciplineId={disciplineId}");
        if (includeInactive) queryParams.Add("includeInactive=true");
        
        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        return await GetAsync<List<PredictionWithDetails>>($"/api/predictions/user/{userId}/details{queryString}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<PredictionResponse>> SubmitPredictionAsync(SubmitPredictionRequest request)
    {
        return await PostAsync<PredictionResponse>("/api/predictions", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<LatestScoredPredictionResponse>> GetLatestScoredPredictionFromActiveAsync(string userId)
    {
        return await GetAsync<LatestScoredPredictionResponse>($"/api/predictions/user/{userId}/latest-scored-active").ConfigureAwait(false);
    }

    // Leaderboard
    public async Task<ApiResponse<List<UserStatistics>>> GetLeaderboardAsync(string seasonId)
    {
        return await GetAsync<List<UserStatistics>>($"/api/leaderboard/season/{seasonId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<UserStatistics>> GetUserStatisticsAsync(string seasonId, string userId)
    {
        return await GetAsync<UserStatistics>($"/api/leaderboard/season/{seasonId}/user/{userId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<EventResultDetails>> GetLastEventResultAsync(string seasonId)
    {
        return await GetAsync<EventResultDetails>($"/api/leaderboard/season/{seasonId}/last-event-result").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<UserEventPrediction>>> SearchLastEventResultAsync(string seasonId, string query)
    {
        return await GetAsync<List<UserEventPrediction>>($"/api/leaderboard/season/{seasonId}/last-event-result/search?query={Uri.EscapeDataString(query)}").ConfigureAwait(false);
    }

    // Admin - Season Management
    public async Task<ApiResponse<MessageResponse>> SetActiveSeasonAsync(string seriesId, string seasonId)
    {
        return await PostAsync<MessageResponse>($"/api/admin/series/{seriesId}/seasons/{seasonId}/set-active", new { }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> FindDuplicateActiveSeasonsAsync()
    {
        return await GetAsync<MessageResponse>("/api/admin/diagnostics/duplicate-active-seasons").ConfigureAwait(false);
    }

    // Admin - Admin Management
    public async Task<ApiResponse<AdminStatusResponse>> GetMyAdminStatusAsync()
    {
        return await GetAsync<AdminStatusResponse>("/api/admin/me").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Admin>>> GetAllAdminsAsync()
    {
        return await GetAsync<List<Admin>>("/api/admin/admins").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Admin>> GetAdminAsync(string userId)
    {
        return await GetAsync<Admin>($"/api/admin/admins/{userId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> CreateAdminAsync(string userId, bool isActive, bool canManageAdmins)
    {
        return await PostAsync<MessageResponse>("/api/admin/admins", 
            new { userId, isActive, canManageAdmins }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdateAdminAsync(string userId, bool isActive, bool canManageAdmins)
    {
        return await PutAsync<MessageResponse>($"/api/admin/admins/{userId}", 
            new { isActive, canManageAdmins }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> RemoveAdminAsync(string userId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/admins/{userId}").ConfigureAwait(false);
    }

    // Admin - Discipline Management
    public async Task<ApiResponse<List<Discipline>>> GetAllDisciplinesAdminAsync()
    {
        return await GetAsync<List<Discipline>>("/api/admin/disciplines").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Discipline>> GetDisciplineAdminAsync(string disciplineId)
    {
        return await GetAsync<Discipline>($"/api/admin/disciplines/{disciplineId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Discipline>> CreateDisciplineAsync(CreateDisciplineRequest request)
    {
        return await PostAsync<Discipline>("/api/admin/disciplines", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<Discipline>> UpdateDisciplineAsync(string disciplineId, UpdateDisciplineRequest request)
    {
        return await PutAsync<Discipline>($"/api/admin/disciplines/{disciplineId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteDisciplineAsync(string disciplineId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/disciplines/{disciplineId}").ConfigureAwait(false);
    }

    // Admin - Series Management
    public async Task<ApiResponse<List<Series>>> GetAllSeriesAdminAsync(string disciplineId)
    {
        return await GetAsync<List<Series>>($"/api/admin/disciplines/{disciplineId}/series").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Series>> GetSeriesAdminAsync(string seriesId)
    {
        return await GetAsync<Series>($"/api/admin/series/{seriesId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Series>> CreateSeriesAsync(CreateSeriesRequest request)
    {
        return await PostAsync<Series>("/api/admin/series", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<Series>> UpdateSeriesAsync(string seriesId, string currentDisciplineId, UpdateSeriesRequest request)
    {
        return await PutAsync<Series>($"/api/admin/series/{seriesId}?currentDisciplineId={currentDisciplineId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteSeriesAsync(string seriesId, string disciplineId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/series/{seriesId}?disciplineId={disciplineId}").ConfigureAwait(false);
    }

    // Admin - Season Management
    public async Task<ApiResponse<List<Season>>> GetAllSeasonsAdminAsync(string seriesId)
    {
        return await GetAsync<List<Season>>($"/api/admin/series/{seriesId}/seasons").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Season>> GetSeasonAdminAsync(string seasonId, string seriesId)
    {
        return await GetAsync<Season>($"/api/admin/seasons/{seasonId}?seriesId={seriesId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<SeasonDependencies>> GetSeasonDependenciesAsync(string seasonId)
    {
        return await GetAsync<SeasonDependencies>($"/api/admin/seasons/{seasonId}/dependencies").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Season>> CreateSeasonAsync(CreateSeasonRequest request)
    {
        return await PostAsync<Season>("/api/admin/seasons", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<Season>> UpdateSeasonAsync(string seasonId, string currentSeriesId, UpdateSeasonRequest request)
    {
        return await PutAsync<Season>($"/api/admin/seasons/{seasonId}?currentSeriesId={currentSeriesId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteSeasonAsync(string seasonId, string seriesId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/seasons/{seasonId}?seriesId={seriesId}").ConfigureAwait(false);
    }

    // Admin - Competitor Management
    public async Task<ApiResponse<List<Competitor>>> GetAllCompetitorsAdminAsync()
    {
        return await GetAsync<List<Competitor>>($"/api/admin/competitors").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Competitor>>> GetCompetitorsByTypeAsync(string type)
    {
        return await GetAsync<List<Competitor>>($"/api/admin/competitors/type/{type}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Competitor>> GetCompetitorAdminAsync(string competitorId)
    {
        return await GetAsync<Competitor>($"/api/admin/competitors/{competitorId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<CompetitorDependencies>> GetCompetitorDependenciesAsync(string competitorId)
    {
        return await GetAsync<CompetitorDependencies>($"/api/admin/competitors/{competitorId}/dependencies").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<string>>> GetCompetitorSeasonsAsync(string competitorId)
    {
        return await GetAsync<List<string>>($"/api/admin/competitors/{competitorId}/seasons").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Competitor>> CreateCompetitorAsync(CreateCompetitorRequest request)
    {
        return await PostAsync<Competitor>("/api/admin/competitors", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<Competitor>> UpdateCompetitorAsync(string competitorId, UpdateCompetitorRequest request)
    {
        return await PutAsync<Competitor>($"/api/admin/competitors/{competitorId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteCompetitorAsync(string competitorId, string type)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/competitors/{competitorId}?type={type}").ConfigureAwait(false);
    }

    // Admin - Season Competitor Management
    public async Task<ApiResponse<List<SeasonCompetitor>>> GetSeasonCompetitorsAdminAsync(string seasonId)
    {
        return await GetAsync<List<SeasonCompetitor>>($"/api/admin/seasons/{seasonId}/competitors").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> AddCompetitorToSeasonAsync(string seasonId, string competitorId)
    {
        return await PostAsync<MessageResponse>($"/api/admin/seasons/{seasonId}/competitors/{competitorId}", new { }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> RemoveCompetitorFromSeasonAsync(string seasonId, string competitorId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/seasons/{seasonId}/competitors/{competitorId}").ConfigureAwait(false);
    }

    // Admin - Event Management
    public async Task<ApiResponse<List<Event>>> GetAllEventsAsync()
    {
        return await GetAsync<List<Event>>("/api/admin/events").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<Event>>> GetEventsBySeasonAdminAsync(string seasonId)
    {
        return await GetAsync<List<Event>>($"/api/admin/seasons/{seasonId}/events").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Event>> GetEventAdminAsync(string eventId)
    {
        return await GetAsync<Event>($"/api/admin/events/{eventId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<EventDependencies>> GetEventDependenciesAsync(string eventId)
    {
        return await GetAsync<EventDependencies>($"/api/admin/events/{eventId}/dependencies").ConfigureAwait(false);
    }

    public async Task<ApiResponse<object>> GetNextEventNumberAsync(string seasonId)
    {
        return await GetAsync<object>($"/api/admin/seasons/{seasonId}/events/next-number").ConfigureAwait(false);
    }

    public async Task<ApiResponse<Event>> CreateEventAsync(CreateEventRequest request)
    {
        return await PostAsync<Event>("/api/admin/events", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<Event>> UpdateEventAsync(string eventId, UpdateEventRequest request)
    {
        return await PutAsync<Event>($"/api/admin/events/{eventId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteEventAsync(string eventId, string seasonId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/events/{eventId}?seasonId={seasonId}").ConfigureAwait(false);
    }

    // Admin - Event Result Management
    public async Task<ApiResponse<EventResult>> GetEventResultAdminAsync(string eventId)
    {
        return await GetAsync<EventResult>($"/api/admin/events/{eventId}/result").ConfigureAwait(false);
    }

    public async Task<ApiResponse<EventResult>> CreateOrUpdateEventResultAsync(string eventId, CreateEventResultRequest request)
    {
        return await PostAsync<EventResult>($"/api/admin/events/{eventId}/result", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteEventResultAsync(string eventId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/events/{eventId}/result").ConfigureAwait(false);
    }

    // Admin - Scoring & Predictions
    public async Task<ApiResponse<ScoringRules>> GetScoringRulesAsync(string seasonId)
    {
        return await GetAsync<ScoringRules>($"/api/scoring/{seasonId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdatePredictionPointsAsync(string eventId, string userId, int points)
    {
        return await PutAsync<MessageResponse>($"/api/admin/predictions/{eventId}/{userId}/points", new { points }).ConfigureAwait(false);
    }

    // Admin - Statistics Recalculation
    public async Task<ApiResponse<RecalculationJobResponse>> StartStatisticsRecalculationAsync(string seasonId)
    {
        return await PostAsync<RecalculationJobResponse>($"/api/admin/seasons/{seasonId}/recalculate-statistics", new { }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<JobStatusResponse>> GetStatisticsJobStatusAsync(string jobId)
    {
        return await GetAsync<JobStatusResponse>($"/api/admin/statistics-jobs/{jobId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<StatisticsUpdateVerification>> VerifyStatisticsUpdatedAsync(string seasonId, DateTime afterTimestamp)
    {
        var timestampStr = afterTimestamp.ToString("o");
        return await GetAsync<StatisticsUpdateVerification>($"/api/admin/seasons/{seasonId}/statistics-updated?afterTimestamp={Uri.EscapeDataString(timestampStr)}").ConfigureAwait(false);
    }

    // Admin - User Management
    public async Task<ApiResponse<List<User>>> GetAllUsersAsync()
    {
        return await GetAsync<List<User>>("/api/admin/users").ConfigureAwait(false);
    }

    public async Task<ApiResponse<List<UserSearchResult>>> SearchUsersAsync(string searchTerm)
    {
        return await GetAsync<List<UserSearchResult>>($"/api/admin/users/search?q={Uri.EscapeDataString(searchTerm)}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<User>> GetUserAdminAsync(string userId)
    {
        return await GetAsync<User>($"/api/admin/users/{userId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<UserDependencies>> GetUserDependenciesAsync(string userId)
    {
        return await GetAsync<UserDependencies>($"/api/admin/users/{userId}/dependencies").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        return await PutAsync<MessageResponse>($"/api/admin/users/{userId}", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> DeleteUserAsync(string userId)
    {
        return await DeleteAsync<MessageResponse>($"/api/admin/users/{userId}").ConfigureAwait(false);
    }

    // Profile Management
    public async Task<ApiResponse<UserProfileResponse>> GetMyProfileAsync()
    {
        return await GetAsync<UserProfileResponse>("/api/profile").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdateUsernameAsync(UpdateUsernameRequest request)
    {
        return await PostAsync<MessageResponse>("/api/profile/username", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdateAuthMethodAsync(UpdateAuthMethodRequest request)
    {
        return await PostAsync<MessageResponse>("/api/profile/auth-method", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdatePasswordAsync(UpdatePasswordRequest request)
    {
        return await PostAsync<MessageResponse>("/api/profile/password", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> SendEmailUpdateOtpAsync(string newEmail)
    {
        return await PostAsync<MessageResponse>("/api/profile/email/send-otp", new { newEmail }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> ConfirmEmailUpdateAsync(ConfirmEmailUpdateRequest request)
    {
        return await PostAsync<MessageResponse>("/api/profile/email/confirm", request).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> SendPasswordSetupOtpAsync()
    {
        return await PostAsync<MessageResponse>("/api/profile/password/send-otp", new { }).ConfigureAwait(false);
    }

    // Language
    public async Task<ApiResponse<List<Language>>> GetLanguagesAsync()
    {
        return await GetAsync<List<Language>>("/api/profile/languages").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> UpdateMyLanguageAsync(string languageCode)
    {
        return await PostAsync<MessageResponse>("/api/profile/language", new { languageCode }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<UserLanguageResponse>> GetMyLanguageAsync()
    {
        return await GetAsync<UserLanguageResponse>("/api/profile/language").ConfigureAwait(false);
    }

    // Favorite Seasons
    public async Task<ApiResponse<List<FavoriteSeason>>> GetFavoriteSeasonsAsync()
    {
        return await GetAsync<List<FavoriteSeason>>("/api/favorites/seasons").ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> AddFavoriteSeasonAsync(string seasonId, string seasonName, string seriesName, int year)
    {
        return await PostAsync<MessageResponse>($"/api/favorites/seasons/{seasonId}", 
            new { seasonName, seriesName, year }).ConfigureAwait(false);
    }

    public async Task<ApiResponse<MessageResponse>> RemoveFavoriteSeasonAsync(string seasonId)
    {
        return await DeleteAsync<MessageResponse>($"/api/favorites/seasons/{seasonId}").ConfigureAwait(false);
    }

    public async Task<ApiResponse<CheckFavoriteResponse>> CheckFavoriteSeasonAsync(string seasonId)
    {
        return await GetAsync<CheckFavoriteResponse>($"/api/favorites/seasons/{seasonId}/check").ConfigureAwait(false);
    }

    // Helper methods
    private async Task<ApiResponse<T>> GetAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
                return new ApiResponse<T> { Success = true, Data = data };
            }

            var error = await ParseErrorResponseAsync(response).ConfigureAwait(false);
            return new ApiResponse<T> { Success = false, Error = error };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    private async Task<ApiResponse<T>> PostAsync<T>(string url, object data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, data).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
                return new ApiResponse<T> { Success = true, Data = result };
            }

            var error = await ParseErrorResponseAsync(response).ConfigureAwait(false);
            return new ApiResponse<T> { Success = false, Error = error };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    private async Task<ApiResponse<T>> PutAsync<T>(string url, object data)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(url, data).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
                return new ApiResponse<T> { Success = true, Data = result };
            }

            var error = await ParseErrorResponseAsync(response).ConfigureAwait(false);
            return new ApiResponse<T> { Success = false, Error = error };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    private async Task<ApiResponse<T>> DeleteAsync<T>(string url)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(url).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>().ConfigureAwait(false);
                return new ApiResponse<T> { Success = true, Data = result };
            }

            var error = await ParseErrorResponseAsync(response).ConfigureAwait(false);
            return new ApiResponse<T> { Success = false, Error = error };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    private async Task<string> ParseErrorResponseAsync(HttpResponseMessage response)
    {
        try
        {
            // Handle specific status codes with user-friendly messages
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return _localizer["Auth_SessionExpired"];
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return _localizer["Auth_Forbidden"];
            }

            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            
            // Try to parse as JSON and extract the error message
            using var doc = JsonDocument.Parse(errorContent);
            if (doc.RootElement.TryGetProperty("error", out var errorProperty))
            {
                return errorProperty.GetString() ?? errorContent;
            }
            
            // If no "error" property, return the raw content
            return errorContent;
        }
        catch
        {
            // If parsing fails, return a generic error message
            return _localizer["General_GenericError"];
        }
    }
}

// Response DTOs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public record RegisterVerificationResponse(string TempUserId, string Message);
public record RegisterResponse(string UserId, string Message);
public record SendOtpResponse(string Message, string Email);
public record MessageResponse(string Message);
public record AuthResponse(string UserId, string Username, string SessionId, string? LanguageCode, string Message);
public record PredictionResponse(string Message, Prediction Prediction);
public record AdminStatusResponse(bool IsAdmin, bool CanManageAdmins);

public record SubmitPredictionRequest(
    string EventId,
    string SeasonId,
    string UserId,
    string FirstPlaceId,
    string FirstPlaceName,
    string SecondPlaceId,
    string SecondPlaceName,
    string ThirdPlaceId,
    string ThirdPlaceName
);

// Admin Request DTOs
public record CreateDisciplineRequest(string Name, string DisplayName, bool IsActive);
public record UpdateDisciplineRequest(string Name, string DisplayName, bool IsActive);

public record CreateSeasonRequest(string SeriesId, int Year, string Name, bool IsActive, DateTime StartDate, DateTime? EndDate, int? BestResultsNumber);
public record UpdateSeasonRequest(string SeriesId, int Year, string Name, bool IsActive, DateTime StartDate, DateTime? EndDate, int? BestResultsNumber);

public record CreateCompetitorRequest(string Name, string ShortName, string Type, bool IsActive);
public record UpdateCompetitorRequest(string Name, string ShortName, string Type, bool IsActive);

public record CreateSeriesRequest(string DisciplineId, string Name, string DisplayName, bool IsActive, string? GoverningBody, string? Region, string? VehicleType);
public record UpdateSeriesRequest(string DisciplineId, string Name, string DisplayName, bool IsActive, string? GoverningBody, string? Region, string? VehicleType);

public record CreateEventRequest(string SeasonId, string Name, string DisplayName, int EventNumber, DateTime EventDate, string Location, string Status, bool IsActive);
public record UpdateEventRequest(string Name, string DisplayName, int EventNumber, DateTime EventDate, string Location, string Status, bool IsActive);

public record CreateEventResultRequest(string FirstPlaceId, string FirstPlaceName, string SecondPlaceId, string SecondPlaceName, string ThirdPlaceId, string ThirdPlaceName);

public record UpdatePointsRequest(int Points);

public record UpdateUserRequest(string Username, string Email, bool IsActive);

public record UserSearchResult(string UserId, string Email, string Username, bool IsActive);

public record RecalculationJobResponse(string Message, string JobId);

public record JobStatusResponse(bool Found, StatisticsRecalculationJob? Job);

public record StatisticsUpdateVerification(int UpdatedCount, bool HasUpdates);

// Profile DTOs
public record UserProfileResponse(
    string UserId, 
    string Email, 
    string Username, 
    string PreferredAuthMethod,
    bool HasPassword,
    bool HasEmail);

public record UpdateUsernameRequest(string NewUsername, string? Password, string? OtpCode);
public record UpdateAuthMethodRequest(string NewAuthMethod, string? Password, string? OtpCode);
public record UpdatePasswordRequest(string? OldPassword, string? OtpCode, string NewPassword);
public record ConfirmEmailUpdateRequest(string NewEmail, string OtpCode);

// Favorite Season DTOs
public record CheckFavoriteResponse(bool IsFavorite);

// Language DTOs
public record UserLanguageResponse(string LanguageCode);
