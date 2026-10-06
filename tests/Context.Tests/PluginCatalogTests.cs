using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Plugin;

namespace Context.Tests;

public class PluginCatalogTests
{
    private sealed class FakeProvider : ICommandProvider
    {
        [HotkeyCommand("fake.next", "下一张", "F9")]
        public IRelayCommand NextCommand { get; } = new RelayCommand(() => { });

        [HotkeyCommand("fake.notACommand", "非命令属性", "F10")]
        public string? NotACommand { get; } = null;

        public IRelayCommand UndecoratedCommand { get; } = new RelayCommand(() => { });

        [PluginSetting("fake.cacheDirectory", "缓存目录")]
        public string CacheDirectory { get; set; } = @"C:\Music";

        [PluginSetting("fake.readOnly", "只读设置")]
        public string ReadOnlySetting { get; } = "固定值";

        [PluginSetting("fake.notString", "非字符串设置")]
        public int NotString { get; set; }

        public string UndecoratedSetting { get; set; } = string.Empty;
    }

    [Fact]
    public void GetCommands_CollectsAttributedICommandProperties_WithMetadata()
    {
        var provider = new FakeProvider();
        var catalog = new PluginCatalog([provider]);

        var commands = catalog.GetCommands();

        var declaration = Assert.Single(commands);
        Assert.Equal("fake.next", declaration.Id);
        Assert.Equal("下一张", declaration.DisplayName);
        Assert.Equal("F9", declaration.DefaultKeyGesture);
        Assert.Same(provider.NextCommand, declaration.Command);
    }

    [Fact]
    public void GetSettings_CollectsReadWriteStringProperties_WithDefaultValueAndSetter()
    {
        var provider = new FakeProvider();
        var catalog = new PluginCatalog([provider]);

        var settings = catalog.GetSettings();

        var declaration = Assert.Single(settings);
        Assert.Equal("fake.cacheDirectory", declaration.Id);
        Assert.Equal("缓存目录", declaration.DisplayName);
        Assert.Equal(@"C:\Music", declaration.DefaultValue);

        declaration.SetValue(@"D:\Audio");
        Assert.Equal(@"D:\Audio", provider.CacheDirectory);
    }
}
