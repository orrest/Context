using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Tests;

/// <summary>
/// 主题字典运行时可加载性兜底：合并字典的 pack URI 仅在运行时解析（编译期不校验），
/// 目录重组/移动文件后由本测试验证全部字典链路（总字典 → 组件字典 → 画刷键）。
/// 与 OWindowTests.DefaultStyle_LoadsWithTemplate 相同：直接在测试线程加载字典即可
/// （pack 解析依赖下方静态构造函数完成的 WPF 初始化）。
/// </summary>
public class ThemeDictionaryTests
{
    static ThemeDictionaryTests()
    {
        // 触发 pack URI 注册：在 STA 线程构造 Window 完成 WPF 初始化（application:// 前缀
        // 随之注册）。仅触碰 PackUriHelper.UriSchemePack 在 .NET (Core) 上不可靠——
        // 单独运行本类（无其他测试类先行初始化 WPF）解析 pack:// 会报
        // "The URI prefix is not recognized"。与 OWindowTests.RunInSta 同思路。
        RunInSta(() => _ = new Controls.OWindow());
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
        Assert.IsType<SolidColorBrush>(theme["ComboBox.Background"]);
        Assert.IsType<SolidColorBrush>(theme["ComboBox.FocusedBorder"]);
        Assert.IsType<SolidColorBrush>(theme["ComboBox.Glyph"]);
        Assert.IsType<SolidColorBrush>(theme["TextBox.Background"]);
        Assert.IsType<SolidColorBrush>(theme["TextBox.FocusedBorder"]);
        Assert.IsType<SolidColorBrush>(theme["CheckBox.Accent"]);
        Assert.IsType<SolidColorBrush>(theme["CheckBox.Checkmark"]);
        Assert.IsType<SolidColorBrush>(theme["TabItem.Foreground"]);
        Assert.IsType<SolidColorBrush>(theme["TabItem.SelectedUnderline"]);
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

        // 六个组件样式字典均已合并：ContextMenu、ComboBox、TextBox、CheckBox、TabControl 与 OWindow 均以类型键暴露样式
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.ContextMenu)]);
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.ComboBox)]);
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.TextBox)]);
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.CheckBox)]);
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.TabControl)]);
        Assert.IsType<Style>(generic[typeof(System.Windows.Controls.TabItem)]);
        Assert.IsType<Style>(generic[typeof(Controls.OWindow)]);
    }

    private static void RunInSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }
}
