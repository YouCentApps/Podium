using Podium.Shared.Services.Data;
using Podium.Shared.Services.Auth;
using Podium.Shared.Services.Business;
using Podium.Api.Endpoints;
using Podium.Api.Services;
using Microsoft.Extensions.Localization;
using Podium.Shared;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Localization - resources are co-located with their marker classes in Podium.Shared, no ResourcesPath needed
builder.Services.AddLocalization();

// Configure CORS for web and mobile apps
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Allow all origins for testing with mobile devices and local dev
        options.AddPolicy("AllowPodiumClients", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    }
    else
    {
        // Production: Allow specific domains
        options.AddPolicy("AllowPodiumClients", policy =>
        {
            policy.WithOrigins(
                "https://youcentpodium.azurewebsites.net",           // Your production web app domain
                "https://www.youcentpodium.azurewebsites.net"        // www variant
                                                                  // Add more production domains as needed
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    }
});

// Register Azure Storage factory
var storageUri = builder.Configuration["AzureStorage:StorageUri"] 
    ?? throw new InvalidOperationException("AzureStorage:StorageUri not configured");
var accountName = builder.Configuration["AzureStorage:AccountName"] 
    ?? throw new InvalidOperationException("AzureStorage:AccountName not configured");
var accountKey = builder.Configuration["AzureStorage:AccountKey"] 
    ?? throw new InvalidOperationException("AzureStorage:AccountKey not configured");

builder.Services.AddSingleton<ITableClientFactory>(
    new TableClientFactory(storageUri, accountName, accountKey));

// Register Email Service
var smtpServer = builder.Configuration["EmailSettings:SmtpServer"];
var smtpPort = builder.Configuration.GetValue<int>("EmailSettings:SmtpPort", 587);
var smtpUsername = builder.Configuration["EmailSettings:Username"];
var smtpPassword = builder.Configuration["EmailSettings:Password"];
var senderEmail = builder.Configuration["EmailSettings:SenderEmail"];
var senderName = builder.Configuration["EmailSettings:SenderName"] ?? "Podium";

if (!string.IsNullOrEmpty(smtpServer) && !string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
{
    builder.Services.AddScoped<IEmailService>(sp =>
    {
        var localizer = sp.GetRequiredService<IStringLocalizer<ApiMessages>>();
        return new EmailService(smtpServer, smtpPort, smtpUsername, smtpPassword, senderEmail ?? smtpUsername, senderName, localizer);
    });
    Console.WriteLine("✅ Email service configured");
}
else
{
    Console.WriteLine("⚠️ Email service not configured - OTP codes will be logged to console only");
}

// Register repositories
builder.Services.AddScoped<IDisciplineRepository, DisciplineRepository>();
builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
builder.Services.AddScoped<ICompetitorRepository, CompetitorRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IPredictionRepository, PredictionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IScoringRulesRepository, ScoringRulesRepository>();
builder.Services.AddScoped<IStatisticsJobRepository, StatisticsJobRepository>();
builder.Services.AddScoped<IFavoriteSeasonRepository, FavoriteSeasonRepository>();

// Register business services
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<IStatisticsRecalculationService, StatisticsRecalculationService>();

// Register authentication services with email callback
builder.Services.AddScoped<IAuthenticationService>(sp =>
{
    var tableClientFactory = sp.GetRequiredService<ITableClientFactory>();
    var userRepository = sp.GetRequiredService<IUserRepository>();
    var emailService = sp.GetService<IEmailService>();
    var localizer = sp.GetRequiredService<IStringLocalizer<ApiMessages>>();

    Action<string, string>? emailCallback = null;
    if (emailService != null)
    {
        emailCallback = (email, code) =>
        {
            _ = emailService.SendVerificationEmailAsync(email, code);
        };
    }

    return new AuthenticationService(tableClientFactory, userRepository, emailCallback, localizer);
});

builder.Services.AddScoped<IRegistrationService>(sp =>
{
    var userRepository = sp.GetRequiredService<IUserRepository>();
    var tableClientFactory = sp.GetRequiredService<ITableClientFactory>();
    var emailService = sp.GetService<IEmailService>();
    var localizer = sp.GetRequiredService<IStringLocalizer<ApiMessages>>();

    Action<string, string>? emailCallback = null;
    if (emailService != null)
    {
        emailCallback = (email, code) =>
        {
            _ = emailService.SendVerificationEmailAsync(email, code);
        };
    }

    return new RegistrationService(userRepository, tableClientFactory, emailCallback, localizer);
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors("AllowPodiumClients");

// Enable request localization - reads Accept-Language header and sets CultureInfo for the request
// Keep this list in sync with ProfileEndpoints.SupportedLanguages when adding new languages.
var supportedCultures = ProfileEndpoints.SupportedLanguages.Select(l => l.Code).ToArray();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("en")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

// Add a simple health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow, environment = app.Environment.EnvironmentName }))
    .WithName("HealthCheck");

// Map API endpoints
app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapSportEndpoints();
app.MapPredictionEndpoints();
app.MapLeaderboardEndpoints();
app.MapAdminEndpoints();
app.MapFavoriteSeasonEndpoints();

app.Run();
