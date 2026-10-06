using Microsoft.Extensions.DependencyInjection;
using Music;
using Music.Commands;
using Music.Player;
using Music.Services;
using Music.Settings;
using Plugin;

namespace Context.Tests;

public class MusicDependencyInjectionTests
{
    [Fact]
    public void AddMusic_RegistersResolvableServices()
    {
        var services = new ServiceCollection();
        services.AddMusic();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<MusicOptions>());
        // 播放器构造仅存引用（引擎随取曲延迟创建），非 STA 单测可安全解析
        Assert.NotNull(provider.GetRequiredService<AudioPlayer>());
        Assert.NotNull(provider.GetRequiredService<ITrackSource>());
        // 覆盖层视图需 STA 构造（WPF），单测仅验证注册存在，不触发实例化
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IMainWindowOverlay));
        // 命令与设置两个提供者经标记接口注册，供宿主插件目录多实现收集
        var providers = provider.GetServices<ICommandProvider>().ToList();
        Assert.Equal(2, providers.Count);
        Assert.IsType<MusicCommands>(providers[0]);
        Assert.IsType<MusicSettings>(providers[1]);
    }
}
