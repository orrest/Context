using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Orrest.Ui.Controls;

namespace Orrest.Ui.Tests;

public class OWindowTests
{
    [Fact]
    public void WindowStyle_CanBeSetAndReadBackUnchanged()
    {
        // OWindow 不强制/不改写 WindowStyle：外部设置原生保真（无 coercion）
        RunInSta(() =>
        {
            var window = new OWindow { WindowStyle = WindowStyle.ToolWindow };

            Assert.Equal(WindowStyle.ToolWindow, window.WindowStyle);
        });
    }

    [Fact]
    public void ShowMinimizeButton_DefaultsToTrue()
    {
        Assert.Equal(true, OWindow.ShowMinimizeButtonProperty.GetMetadata(typeof(OWindow)).DefaultValue);
    }

    [Fact]
    public void ShowMaximizeButton_DefaultsToTrue()
    {
        Assert.Equal(true, OWindow.ShowMaximizeButtonProperty.GetMetadata(typeof(OWindow)).DefaultValue);
    }

    [Fact]
    public void ShowButtons_CanBeSetAndRead()
    {
        // WPF Window 构造函数要求 STA 线程；xUnit 默认线程池是 MTA
        RunInSta(() =>
        {
            var window = new OWindow { ShowMinimizeButton = false, ShowMaximizeButton = false };

            Assert.False(window.ShowMinimizeButton);
            Assert.False(window.ShowMaximizeButton);
        });
    }

    [Fact]
    public void WindowBackdropType_DefaultsToAcrylic()
    {
        Assert.Equal(OWindowBackdropType.Acrylic,
            OWindow.WindowBackdropTypeProperty.GetMetadata(typeof(OWindow)).DefaultValue);
    }

    [Fact]
    public void OWindowBackdropType_ValuesMatchDwmSystemBackdrop()
    {
        // 数值直接传给 DwmSetWindowAttribute 的 DWMSBT
        Assert.Equal(1, (int)OWindowBackdropType.None);
        Assert.Equal(2, (int)OWindowBackdropType.Mica);
        Assert.Equal(3, (int)OWindowBackdropType.Acrylic);
    }

    [Fact]
    public void DefaultStyle_LoadsWithTemplate()
    {
        // 资源字典可加载（XAML 解析无误）且默认样式带模板；
        // 未显示的 Window 在测试中不套用模板，完整渲染由应用冒烟验证
        var dictionary = new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/Orrest.Ui;component/Styles/Window.xaml", UriKind.Absolute),
        };

        var style = Assert.IsType<Style>(dictionary[typeof(OWindow)]);
        var template = style.Setters
            .OfType<Setter>()
            .FirstOrDefault(setter => setter.Property == Window.TemplateProperty);

        Assert.NotNull(template);
        Assert.IsType<ControlTemplate>(template.Value);
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
