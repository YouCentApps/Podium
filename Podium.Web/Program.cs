using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Podium.Shared;
using Podium.Shared.Services.Api;
using Podium.Shared.Services.State;
using Podium.Shared.Services.Configuration;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure app settings
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:50001";
var isDevelopment = builder.HostEnvironment.IsDevelopment();

var appConfig = new AppConfiguration 
{ 
    ApiBaseUrl = apiBaseUrl,
    IsDevelopment = isDevelopment
};

builder.Services.AddSingleton<IAppConfiguration>(appConfig);

// Localization - resources are co-located with their marker classes, no ResourcesPath needed
builder.Services.AddLocalization();

builder.Services.AddScoped<IStorageService, BrowserStorageService>();
builder.Services.AddScoped<AuthStateService>(sp =>
{
    var storageService = sp.GetRequiredService<IStorageService>();
    return new AuthStateService(storageService);
});

builder.Services.AddScoped<LanguageStateService>(sp =>
{
    var storageService = sp.GetRequiredService<IStorageService>();
    return new LanguageStateService(storageService);
});

// Register the authentication message handler
builder.Services.AddScoped<AuthenticationMessageHandler>();

// Configure HttpClient with API base URL and authentication handler
builder.Services.AddScoped(sp => 
{
    var config = sp.GetRequiredService<IAppConfiguration>();
    var authHandler = sp.GetRequiredService<AuthenticationMessageHandler>();
    authHandler.InnerHandler = new HttpClientHandler();

    return new HttpClient(authHandler) { BaseAddress = new Uri(config.ApiBaseUrl) };
});

builder.Services.AddScoped<IPodiumApiClient, PodiumApiClient>();
builder.Services.AddScoped<AdminStateService>();

var host = builder.Build();

// Bootstrap culture from localStorage BEFORE the app renders
var js = host.Services.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
var languageCode = await js.InvokeAsync<string>("podiumCulture.get", Array.Empty<object>());
if (!string.IsNullOrWhiteSpace(languageCode))
{
    var culture = new CultureInfo(languageCode);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

// Initialize language state (also sets CurrentLanguageCode in service)
var languageState = host.Services.GetRequiredService<LanguageStateService>();
await languageState.InitializeAsync();

// Initialize auth state (restore session from storage)
var authState = host.Services.GetRequiredService<AuthStateService>();
await authState.InitializeAsync();

await host.RunAsync();
