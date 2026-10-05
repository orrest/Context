using Plugin;
using Wallpaper;
using Wallpaper.Commands;
using Wallpaper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Tests;

public class WallpaperDependencyInjectionTests
{
    [Fact]
    public void AddWallpaper_RegistersResolvableServices()
    {
        var services = new ServiceCollection();
        services.AddWallpaper();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<WallpaperService>());
        // 命令提供者经标记接口注册，供宿主命令目录多实现收集
        var providerInstance = Assert.IsType<WallpaperCommands>(
            provider.GetRequiredService<ICommandProvider>());
        Assert.NotNull(providerInstance.NextCommand);
    }
}
