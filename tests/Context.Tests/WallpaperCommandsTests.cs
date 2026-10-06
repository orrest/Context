using Plugin;
using Wallpaper.Abstractions;
using Wallpaper.Commands;
using Wallpaper.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperCommandsTests
{
    [Fact]
    public void Catalog_AggregatesWallpaperNextDeclaration_WithDefaultAltDown()
    {
        var service = new WallpaperService(
            Substitute.For<IWallpaperRepository>(),
            Substitute.For<IDesktopWallpaperSetter>());
        var provider = new WallpaperCommands(service);
        var catalog = new PluginCatalog([provider]);

        var declaration = Assert.Single(catalog.GetCommands());

        Assert.Equal(WallpaperCommands.NextCommandId, declaration.Id);
        Assert.Equal("壁纸：切换下一张", declaration.DisplayName);
        Assert.Equal("Alt+Down", declaration.DefaultKeyGesture);
        Assert.Same(provider.NextCommand, declaration.Command);
    }
}
