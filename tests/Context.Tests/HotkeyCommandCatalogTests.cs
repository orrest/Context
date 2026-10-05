using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Plugin;

namespace Context.Tests;

public class HotkeyCommandCatalogTests
{
    private sealed class FakeProvider : ICommandProvider
    {
        [HotkeyCommand("fake.next", "下一张", "F9")]
        public IRelayCommand NextCommand { get; } = new RelayCommand(() => { });

        [HotkeyCommand("fake.notACommand", "非命令属性", "F10")]
        public string? NotACommand { get; } = null;

        public IRelayCommand UndecoratedCommand { get; } = new RelayCommand(() => { });
    }

    [Fact]
    public void GetCommands_CollectsAttributedICommandProperties_WithMetadata()
    {
        var provider = new FakeProvider();
        var catalog = new HotkeyCommandCatalog([provider]);

        var commands = catalog.GetCommands();

        var declaration = Assert.Single(commands);
        Assert.Equal("fake.next", declaration.Id);
        Assert.Equal("下一张", declaration.DisplayName);
        Assert.Equal("F9", declaration.DefaultKeyGesture);
        Assert.Same(provider.NextCommand, declaration.Command);
    }
}
