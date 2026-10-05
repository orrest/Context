using Context.App.Commands;
using Context.App.Views;
using Plugin;
using Microsoft.Extensions.DependencyInjection;

namespace Context.App;

public static class DependencyInjection
{
    /// <summary>
    /// 快捷键目录装配：命令目录隐式聚合全部 ICommandProvider 声明（设置页与
    /// 主窗口快捷键装配共用）。
    /// </summary>
    public static IServiceCollection AddHotkeys(this IServiceCollection services)
    {
        services.AddSingleton<IHotkeyCommandCatalog, HotkeyCommandCatalog>();
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
