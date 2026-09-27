using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Orrest.Ui.Tray;

/// <summary>
/// 窗口托盘 behavior：附加到 <see cref="Window"/> 后，在 SourceInitialized 时把窗口从
/// Alt+Tab 列表与任务栏中隐藏并注册托盘图标（双击托盘图标激活窗口，右键弹出菜单：退出）；
/// 窗口关闭时移除托盘图标。
/// </summary>
public sealed class TrayIconBehavior : Behavior<Window>
{
    /// <summary>标识 <see cref="Tooltip"/> 依赖属性。</summary>
    public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
        nameof(Tooltip), typeof(string), typeof(TrayIconBehavior), new PropertyMetadata(string.Empty));

    /// <summary>标识 <see cref="ExitMenuHeader"/> 依赖属性。</summary>
    public static readonly DependencyProperty ExitMenuHeaderProperty = DependencyProperty.Register(
        nameof(ExitMenuHeader), typeof(string), typeof(TrayIconBehavior),
        new PropertyMetadata("退出"));

    private TrayIconController? _controller;

    /// <summary>托盘悬停提示文本。</summary>
    public string Tooltip
    {
        get { return (string)GetValue(TooltipProperty); }
        set { SetValue(TooltipProperty, value); }
    }

    /// <summary>托盘菜单中“退出”项的文案。</summary>
    public string ExitMenuHeader
    {
        get { return (string)GetValue(ExitMenuHeaderProperty); }
        set { SetValue(ExitMenuHeaderProperty, value); }
    }

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SourceInitialized += OnSourceInitialized;
        AssociatedObject.Closed += OnClosed;
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        AssociatedObject.SourceInitialized -= OnSourceInitialized;
        AssociatedObject.Closed -= OnClosed;
        DetachController();
        base.OnDetaching();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        // 窗口不出现在 Alt+Tab；托盘图标是唯一的应用入口
        WindowStyles.HideFromAltTab(AssociatedObject);

        _controller = new TrayIconController(AssociatedObject, Tooltip);
        _controller.IconDoubleClicked += (_, _) => AssociatedObject.Activate();
        var exit = new MenuItem { Header = ExitMenuHeader };
        exit.Click += (_, _) => AssociatedObject.Close();
        _controller.Menu.Items.Add(exit);
        _controller.Attach();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        DetachController();
    }

    private void DetachController()
    {
        _controller?.Detach();
        _controller = null;
    }
}
