using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Tests;

/// <summary>
/// 主题字典运行时可加载性兜底：合并字典的 pack URI 仅在运行时解析（编译期不校验），
/// 目录重组/移动文件后由本测试验证全部字典链路（总字典 → 组件字典 → 画刷键）。
/// 与 OWindowTests.DefaultStyle_LoadsWithTemplate 相同：直接在测试线程加载字典即可。
/// </summary>
public class ThemeDictionaryTests
{
    static ThemeDictionaryTests()
    {
        // 触发 pack URI 方案注册（xUnit 并行时未完成注册的线程解析 pack:// 会报
        // "Invalid URI: Invalid port specified"）
        _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
    }

    [Theory]
    [InlineData("Themes/Theme.Light.xaml")]
    [InlineData("Themes/Theme.Dark.xaml")]
    public void TotalThemeDictionary_LoadsWithComponentBrushes(string themePath)
    {
        var theme = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Orrest.Ui;component/{themePath}", UriKind.Absolute),
        };

        Assert.IsType<SolidColorBrush>(theme["ContextMenu.Background"]);
        Assert.IsType<SolidColorBrush>(theme["ContextMenu.Highlight"]);
        Assert.IsType<SolidColorBrush>(theme["Window.Background"]);
        Assert.IsType<SolidColorBrush>(theme["Window.CloseHover"]);
        Assert.IsType<double>(theme["Theme.BackgroundOpacity"]);
    }

    [Fact]
    public void GenericDictionary_LoadsWithAllComponentStyles()
    {
        var generic = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Orrest.Ui;component/Themes/Generic.xaml", UriKind.Absolute),
        };

        // 两个组件样式字典均已合并：ContextMenu 与 OWindow 均以类型键暴露样式
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.ContextMenu)]);
        Assert.IsType<Style>(generic[typeof(Controls.OWindow)]);
    }
}
