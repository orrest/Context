using Context.App.Commands;
using Plugin;

namespace Context.Tests;

public class SettingsCommandsTests
{
    [Fact]
    public void Catalog_AggregatesSettingsOpenDeclaration_WithDefaultAltM()
    {
        // 工厂不被执行：命令执行会构造 WPF Window，需 STA，不在单测中触发
        var provider = new SettingsCommands(
            () => throw new InvalidOperationException("窗口工厂不应在声明测试中调用"));
        var catalog = new HotkeyCommandCatalog([provider]);

        var declaration = Assert.Single(catalog.GetCommands());

        Assert.Equal(SettingsCommands.OpenCommandId, declaration.Id);
        Assert.Equal("设置：打开设置窗口", declaration.DisplayName);
        Assert.Equal("Alt+M", declaration.DefaultKeyGesture);
        Assert.Same(provider.OpenCommand, declaration.Command);
    }
}
