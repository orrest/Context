using Microsoft.Extensions.DependencyInjection;
using Music.Commands;
using Music.Player;
using Music.Services;
using Music.Settings;
using Music.ViewModels;
using Music.Views;
using Plugin;

namespace Music;

public static class DependencyInjection
{
    public static IServiceCollection AddMusic(this IServiceCollection services)
    {
        services.AddSingleton<MusicOptions>();
        services.AddSingleton<ITrackSource, LocalMusicLibrary>();
        services.AddAudioPlayer();

        // 覆盖层视图由主窗口（STA）构造时实例化，故覆盖层以单例注册
        services.AddTransient<MusicViewModel>();
        services.AddTransient<MusicOverlayView>();
        services.AddSingleton<IMainWindowOverlay, MusicOverlay>();

        // 命令与设置提供者经标记接口注册，宿主插件目录（ICommandProvider 多实现收集）隐式聚合；
        // 单例：声明实例被常驻目录聚合，设置项属性委托 MusicOptions 单例读写
        services.AddSingleton<ICommandProvider, MusicCommands>();
        services.AddSingleton<ICommandProvider, MusicSettings>();

        return services;
    }
}
