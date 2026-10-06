using Plugin;
using Wallpaper.Abstractions;
using Wallpaper.Commands;
using Wallpaper.Desktop;
using Wallpaper.Http;
using Wallpaper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Wallpaper;

public static class DependencyInjection
{
    public static IServiceCollection AddWallpaper(
        this IServiceCollection services,
        string bingBaseUrl = "https://cn.bing.com/")
    {
        services.AddSingleton<WallpaperService>();

        // Never register bare HttpClient as a service type: a later registration
        // 禁止将裸 HttpClient 注册为服务类型：按 DI 规则后注册的实例
        // replaces the earlier one, so every consumer would share the last client.
        // 会覆盖先注册的实例，所有消费者都会拿到最后一个客户端。
        services.AddSingleton<IWallpaperRepository>(
            _ => new BingWallpaperRepository(CreateHttpClient(bingBaseUrl)));
        services.AddSingleton<IDesktopWallpaperSetter, WindowsDesktopWallpaperSetter>();

        // 命令提供者经标记接口注册，宿主插件目录（ICommandProvider 多实现收集）隐式聚合；
        // 单例：命令实例被常驻目录聚合
        services.AddSingleton<ICommandProvider, WallpaperCommands>();

        return services;
    }

    private static HttpClient CreateHttpClient(string baseUrl)
    {
        var handler = new SocketsHttpHandler
        {
            // Periodically rebuild the connection pool so long-lived clients pick up DNS changes.
            // 定期重建连接池，避免长生命周期实例的 DNS 记录过期。
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Context/1.0");
        return httpClient;
    }
}
