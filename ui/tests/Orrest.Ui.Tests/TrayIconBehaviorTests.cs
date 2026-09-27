using System.Windows;
using Orrest.Ui.Tray;

namespace Orrest.Ui.Tests;

public class TrayIconBehaviorTests
{
    [Fact]
    public void Tooltip_DefaultsToEmpty()
    {
        var metadata = TrayIconBehavior.TooltipProperty.GetMetadata(typeof(TrayIconBehavior));

        Assert.Equal(string.Empty, metadata.DefaultValue);
    }

    [Fact]
    public void ExitMenuHeader_DefaultsToExit()
    {
        var exit = TrayIconBehavior.ExitMenuHeaderProperty.GetMetadata(typeof(TrayIconBehavior));

        Assert.Equal("退出", exit.DefaultValue);
    }

    [Fact]
    public void AttachAndDetach_DoesNotThrow()
    {
        // WPF 依赖对象要求 STA 线程；xUnit 默认线程池是 MTA
        RunInSta(() =>
        {
            var window = new Window();
            var behavior = new TrayIconBehavior { Tooltip = "测试" };

            behavior.Attach(window);
            behavior.Detach();
        });
    }

    private static void RunInSta(Action action)
    {
        var thread = new Thread(() => action());
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
