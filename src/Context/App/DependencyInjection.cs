using Context.App.Commands;
using Context.App.Views;
using Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Context.App;

public static class DependencyInjection
{
    /// <summary>
    /// 插件目录装配：插件目录隐式聚合全部 ICommandProvider 声明（快捷键命令与插件设置，
    /// 设置页与主窗口装配共用）。
    /// </summary>
    public static IServiceCollection AddHotkeys(this IServiceCollection services)
    {
        services.AddSingleton<IPluginCatalog, PluginCatalog>();
        return services;
    }

    public static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddTransient<SettingsWindow>();
        services.AddSingleton<Func<SettingsWindow>>(
            sp => () => sp.GetRequiredService<SettingsWindow>());

        // 命令提供者经标记接口注册，宿主命令目录（ICommandProvider 多实现收集）隐式聚合；
        // 单例：命令实例被常驻目录聚合，故不持有窗口引用，窗口每次经工厂新建
        services.AddSingleton<ICommandProvider, SettingsCommands>();

        services.AddTransient<MainWindow>();

        return services;
    }
}
