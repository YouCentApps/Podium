namespace Podium.Shared.Services.Configuration;

public interface IAppConfiguration
{
    string ApiBaseUrl { get; }
    bool IsDevelopment { get; }
}

public class AppConfiguration : IAppConfiguration
{
    public string ApiBaseUrl { get; set; } = ApiBaseUrlDevelopment;
    public bool IsDevelopment { get; set; } = true;

    public static string ApiBaseUrlDevelopment = "https://localhost:50001";
    public static string ApiBaseUrlProduction = "https://youcent-podium-api.azurewebsites.net";
}
