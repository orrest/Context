using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Orrest.Ui.Tests;

/// <summary>
/// TabControl 主题模板的布局冒烟：应用 Generic + 明/暗主题字典后真实执行
/// Measure/Arrange——模板里 x:Static 常量类型错误（如 double 填给 Thickness 的 Margin）
/// 只在测量阶段拆箱时抛 InvalidCastException，字典可加载性测试覆盖不到，由本测试兜底。
/// 与 ThemeDictionaryTests 相同：pack 解析依赖静态构造里完成的 WPF 初始化。
/// </summary>
public class TabControlTemplateTests
{
    static TabControlTemplateTests()
    {
        RunInSta(() => _ = new Controls.OWindow());
    }

    [Theory]
    [InlineData("Themes/Theme.Light.xaml")]
    [InlineData("Themes/Theme.Dark.xaml")]
    public void ThemedTabControl_MeasureAndArrange_Completes(string themePath)
    {
        RunInSta(() =>
        {
            var host = new Grid();
            host.Resources.MergedDictionaries.Add(Load("Themes/Generic.xaml"));
            host.Resources.MergedDictionaries.Add(Load(themePath));

            var tabControl = new TabControl();
            tabControl.Items.Add(new TabItem { Header = "通用", Content = new TextBlock { Text = "内容" } });
            tabControl.Items.Add(new TabItem { Header = "快捷键", Content = new TextBox { Text = "F8" } });
            host.Children.Add(tabControl);

            tabControl.Measure(new Size(320, 240));
            tabControl.Arrange(new Rect(0, 0, 320, 240));

            Assert.True(tabControl.ActualWidth > 0);
            Assert.True(tabControl.ActualHeight > 0);
            // 两个 TabItem 均完成布局（模板触发器/指示条路径被执行）
            var item = Assert.IsType<TabItem>(tabControl.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.True(item.ActualHeight > 0);
        });
    }

    [Theory]
    [InlineData("Themes/Theme.Light.xaml")]
    [InlineData("Themes/Theme.Dark.xaml")]
    public void SelectedTab_Foreground_StaysThemeForeground(string themePath)
    {
        RunInSta(() =>
        {
            var host = new Grid();
            host.Resources.MergedDictionaries.Add(Load("Themes/Generic.xaml"));
            var theme = Load(themePath);
            host.Resources.MergedDictionaries.Add(theme);

            // 页内容在逻辑树上是 TabItem 的子级：选中标签的 Foreground 若为强调色，
            // 会继承到页内全部文字——回归验证选中态前景保持主题常规前景色。
            var content = new TextBlock { Text = "内容" };
            var tabControl = new TabControl();
            var tab = new TabItem { Header = "通用", Content = content };
            tabControl.Items.Add(tab);
            tabControl.Items.Add(new TabItem { Header = "快捷键", Content = new TextBlock { Text = "F8" } });
            host.Children.Add(tabControl);

            tabControl.Measure(new Size(320, 240));
            tabControl.Arrange(new Rect(0, 0, 320, 240));
            tabControl.UpdateLayout();

            Assert.True(tab.IsSelected);
            var expected = themePath.Contains("Dark")
                ? Themes.CommonThemeDark.Foreground
                : Themes.CommonThemeLight.Foreground;
            Assert.Equal(expected, ((SolidColorBrush)tab.Foreground).Color);
            Assert.Equal(expected, ((SolidColorBrush)content.Foreground).Color);
        });
    }

    private static ResourceDictionary Load(string path)
    {
        return new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Orrest.Ui;component/{path}", UriKind.Absolute),
        };
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
