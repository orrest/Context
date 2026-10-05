using Dictionary.Commands;
using Dictionary.Views;
using Plugin;

namespace Context.Tests;

public class DictionaryCommandsTests
{
    [Fact]
    public void Catalog_AggregatesDictionaryOpenDeclaration_WithDefaultAltD()
    {
        // 工厂不被执行：命令执行会构造 WPF Window，需 STA，不在单测中触发
        var provider = new DictionaryCommands(
            () => throw new InvalidOperationException("窗口工厂不应在声明测试中调用"));
        var catalog = new HotkeyCommandCatalog([provider]);

        var declaration = Assert.Single(catalog.GetCommands());

        Assert.Equal(DictionaryCommands.OpenCommandId, declaration.Id);
        Assert.Equal("词典：打开词典窗口", declaration.DisplayName);
        Assert.Equal("Alt+D", declaration.DefaultKeyGesture);
        Assert.Same(provider.OpenCommand, declaration.Command);
    }
}
