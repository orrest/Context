using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.Xaml.Behaviors;

namespace Orrest.Ui.Tray;

/// <summary>
/// 窗口托盘 behavior：附加到 <see cref="Window"/> 后，在 SourceInitialized 时把窗口从
/// Alt+Tab 列表与任务栏中隐藏并注册托盘图标（双击托盘图标激活窗口，右键弹出菜单）；
/// 窗口关闭时移除托盘图标。
/// 托盘菜单 = <see cref="MenuItems"/> 声明的自定义菜单项（按声明顺序）+ 分隔线 + 内置「退出」项；
/// 未声明自定义项时菜单仅含「退出」。
/// </summary>
[ContentProperty(nameof(MenuItems))]
public sealed class TrayIconBehavior : Behavior<Window>
{
    /// <summary>标识 <see cref="Tooltip"/> 依赖属性。</summary>
    public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
        nameof(Tooltip), typeof(string), typeof(TrayIconBehavior), new PropertyMetadata(string.Empty));

    /// <summary>标识 <see cref="ExitMenuHeader"/> 依赖属性。</summary>
    public static readonly DependencyProperty ExitMenuHeaderProperty = DependencyProperty.Register(
        nameof(ExitMenuHeader), typeof(string), typeof(TrayIconBehavior),
        new PropertyMetadata("退出"));

    /// <summary>标识 <see cref="MenuItems"/> 依赖属性。</summary>
    public static readonly DependencyProperty MenuItemsProperty = DependencyProperty.Register(
        nameof(MenuItems), typeof(IList), typeof(TrayIconBehavior), new PropertyMetadata(null));

    private TrayIconController? _controller;

    /// <summary>初始化 <see cref="TrayIconBehavior"/>，并为 <see cref="MenuItems"/> 建立逐实例集合。</summary>
    public TrayIconBehavior()
    {
        // 集合依赖属性的元数据默认值会被全部实例共享，故在构造内逐实例初始化
        SetValue(MenuItemsProperty, new List<MenuItem>());
    }

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

    /// <summary>托盘菜单的自定义菜单项（XAML 子元素直接写入；插入到内置「退出」项之前）。</summary>
    public IList MenuItems
    {
        get { return (IList)GetValue(MenuItemsProperty); }
        set { SetValue(MenuItemsProperty, value); }
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

        var customItems = MenuItems.OfType<MenuItem>().ToList();
        foreach (var item in customItems)
        {
            // Behavior 的 MenuItems 不在视觉/逻辑树内，不参与 DataContext 继承；
            // 未显式设置时继承窗口 DataContext，使菜单项的 {Binding} 命令绑定可用
            if (item.DataContext is null)
            {
                item.DataContext = AssociatedObject.DataContext;
            }

            _controller.Menu.Items.Add(item);
        }

        if (customItems.Count > 0)
        {
            _controller.Menu.Items.Add(new Separator());
        }

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
