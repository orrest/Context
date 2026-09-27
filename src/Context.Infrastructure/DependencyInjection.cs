using Context.UseCases.Abstractions;
using Context.Infrastructure.Desktop;
using Context.Infrastructure.Http;
using Context.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString = "Data Source=app.db",
        string bingBaseUrl = "https://cn.bing.com/",
        string icibaBaseUrl = "https://www.iciba.com/")
    {
        services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<DatabaseInitializer>();

        var httpClient = new HttpClient { BaseAddress = new Uri(bingBaseUrl) };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Context/1.0");
        services.AddSingleton(httpClient);
        services.AddSingleton<IWallpaperRepository, BingWallpaperRepository>();
        services.AddSingleton<IDesktopWallpaperSetter, WindowsDesktopWallpaperSetter>();

        var icibaHttpClient = new HttpClient { BaseAddress = new Uri(icibaBaseUrl) };
        icibaHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Context/1.0");
        services.AddSingleton(icibaHttpClient);
        services.AddSingleton<IDictionaryService, IcibaDictionaryService>();

        return services;
    }
}
