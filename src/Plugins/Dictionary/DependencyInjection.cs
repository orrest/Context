using System.Net.Http;
using Dictionary.Abstractions;
using Dictionary.Commands;
using Dictionary.Http;
using Dictionary.ViewModels;
using Dictionary.Views;
using Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Dictionary;

public static class DependencyInjection
{
    public static IServiceCollection AddDictionary(
        this IServiceCollection services,
        string icibaBaseUrl = "https://dict.iciba.com/")
    {
        // Never register bare HttpClient as a service type: a later registration
        // 禁止将裸 HttpClient 注册为服务类型：按 DI 规则后注册的实例
        // replaces the earlier one, so every consumer would share the last client.
        // 会覆盖先注册的实例，所有消费者都会拿到最后一个客户端。
        services.AddSingleton<IDictionaryService>(
            _ => new IcibaDictionaryService(CreateHttpClient(icibaBaseUrl)));
        services.AddTransient<DictionaryViewModel>();
        services.AddTransient<DictionaryWindow>();
        services.AddSingleton<Func<DictionaryWindow>>(
            sp => () => sp.GetRequiredService<DictionaryWindow>());

        // 命令提供者经标记接口注册，宿主命令目录（ICommandProvider 多实现收集）隐式聚合；
        // 单例：命令实例被常驻目录聚合，故不持有窗口引用，窗口每次经工厂新建
        services.AddSingleton<ICommandProvider, DictionaryCommands>();

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
