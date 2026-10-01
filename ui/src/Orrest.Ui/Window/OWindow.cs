using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Orrest.Ui.Interop;
using Orrest.Ui.Themes;

namespace Orrest.Ui.Controls;

/// <summary>DWM 系统背景材质（数值即 DWMSBT，Win11 22H2+ 生效，旧系统静默回退实底）。</summary>
public enum OWindowBackdropType
{
    /// <summary>无背景材质，窗口为实底（取主题画刷），颜色确定、失焦不变色。</summary>
    None = 1,

    /// <summary>Mica：应用级材质，弱透出桌面，失焦仅轻微变暗。</summary>
    Mica = 2,

    /// <summary>
    /// Acrylic：强透视模糊（默认值）。注意 Win11 在窗口失焦时会把材质回退为纯色（常呈黑色）。
    /// </summary>
    Acrylic = 3,
}

/// <summary>
/// 自绘标题栏窗口：原生窗口框与 <see cref="Window.WindowStyle"/> 语义完整保留
/// （SingleBorderWindow/ToolWindow/None 原生生效，可自由设置读回），
/// 原生标题栏内容经 <see cref="WindowChromeInterop"/> 抹除，标题栏由默认模板提供——
/// 拖动、双击最大化走 WindowChrome.CaptionHeight 原生路径，缩放、Aero Snap、
/// 最大化不遮挡任务栏均为原生行为；半透明由 DWM 系统背景（<see cref="WindowBackdropType"/>）提供。
/// 标题栏按钮显隐按 WindowStyle/ResizeMode 收敛：ToolWindow 仅关闭，None 无标题栏。
/// </summary>
public class OWindow : Window
{
    /// <summary>标识 <see cref="WindowBackdropType"/> 依赖属性。</summary>
    public static readonly DependencyProperty WindowBackdropTypeProperty = DependencyProperty.Register(
        nameof(WindowBackdropType), typeof(OWindowBackdropType), typeof(OWindow),
        new PropertyMetadata(OWindowBackdropType.Acrylic, OnWindowBackdropTypeChanged));

    /// <summary>标识 <see cref="ShowMinimizeButton"/> 依赖属性。</summary>
    public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMinimizeButton), typeof(bool), typeof(OWindow), new PropertyMetadata(true));

    /// <summary>标识 <see cref="ShowMaximizeButton"/> 依赖属性。</summary>
    public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
        nameof(ShowMaximizeButton), typeof(bool), typeof(OWindow), new PropertyMetadata(true));

    /// <summary>最小化命令（标题栏按钮使用）。</summary>
    public static readonly RoutedUICommand MinimizeCommand =
        new("最小化", nameof(MinimizeCommand), typeof(OWindow));

    /// <summary>最大化/还原切换命令（标题栏按钮使用）。</summary>
    public static readonly RoutedUICommand MaximizeCommand =
        new("最大化", nameof(MaximizeCommand), typeof(OWindow));

    static OWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OWindow), new FrameworkPropertyMetadata(typeof(OWindow)));
    }

    private WindowChrome? _chrome;

    /// <summary>创建自绘标题栏窗口。</summary>
    public OWindow()
    {
        // Window 子类的默认样式解析会被主题的 Window 样式劫持（Fluent/ThemeMode 下基类型回退，
        // 模板被换成系统窗口模板、标题栏消失），与 WPF-UI FluentWindow 同款：
        // 显式按自身类型引用样式资源，保证默认模板始终取自本程序集 Generic.xaml。
        SetResourceReference(StyleProperty, typeof(OWindow));
        // 每窗口独立实例（WindowChrome 非 Freezable，不共享）。
        // CaptionHeight 承载拖动与双击最大化；ResizeBorderThickness 在 ResizeMode=NoResize
        // 时被原生样式忽略（无 WS_THICKFRAME）；GlassFrameThickness 随背景材质联动，见 UpdateWindowChrome。
        _chrome = new WindowChrome
        {
            CaptionHeight = WindowTheme.CaptionHeight,
            ResizeBorderThickness = new Thickness(4),
            UseAeroCaptionButtons = false,
        };
        WindowChrome.SetWindowChrome(this, _chrome);
        UpdateWindowChrome();

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (_, _) => Close()));
        CommandBindings.Add(new CommandBinding(MinimizeCommand, (_, _) => WindowState = WindowState.Minimized));
        CommandBindings.Add(new CommandBinding(MaximizeCommand, (_, _) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized));

        // 主题切换时同步 DWM 暗色模式（影响背景材质明暗与系统边框）
        ThemeManager.ThemeApplied += OnThemeApplied;
    }

    /// <summary>
    /// DWM 系统背景材质；默认 Acrylic（强透视）。
    /// None 时窗口为实底，其余值窗口背景置透明、由合成器绘制材质（主题明暗经 DWM 暗色模式同步）。
    /// </summary>
    public OWindowBackdropType WindowBackdropType
    {
        get { return (OWindowBackdropType)GetValue(WindowBackdropTypeProperty); }
        set { SetValue(WindowBackdropTypeProperty, value); }
    }

    /// <summary>是否显示最小化按钮（默认 true；ToolWindow 形态模板强制隐藏）。</summary>
    public bool ShowMinimizeButton
    {
        get { return (bool)GetValue(ShowMinimizeButtonProperty); }
        set { SetValue(ShowMinimizeButtonProperty, value); }
    }

    /// <summary>是否显示最大化按钮（默认 true；ToolWindow 或 ResizeMode 不允许时模板强制隐藏）。</summary>
    public bool ShowMaximizeButton
    {
        get { return (bool)GetValue(ShowMaximizeButtonProperty); }
        set { SetValue(ShowMaximizeButtonProperty, value); }
    }

    /// <inheritdoc />
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // 句柄已创建：抹除原生标题栏内容，应用圆角、暗色模式与背景材质
        WindowChromeInterop.StripNativeTitleBar(this);
        WindowChromeInterop.SetDarkMode(this, ThemeManager.Current == AppTheme.Dark);
        ApplyBackdrop();
    }

    /// <inheritdoc />
    protected override void OnClosed(EventArgs e)
    {
        ThemeManager.ThemeApplied -= OnThemeApplied;
        base.OnClosed(e);
    }

    private static void OnWindowBackdropTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OWindow window)
        {
            window.UpdateWindowChrome();
            if (window.IsLoaded)
            {
                window.ApplyBackdrop();
            }
        }
    }

    /// <summary>
    /// DWM 背景材质绘制在玻璃框延伸区内：启用材质时 GlassFrameThickness 须为 -1
    /// （延伸覆盖整个窗口），否则材质无处绘制、透明背景透出黑色窗面；
    /// 实底（None）时取近 0 值，避免绘制玻璃边。
    /// </summary>
    private void UpdateWindowChrome()
    {
        if (_chrome is null)
        {
            return;
        }

        _chrome.GlassFrameThickness = WindowBackdropType == OWindowBackdropType.None
            ? new Thickness(0.00001)
            : new Thickness(-1);
    }

    private void ApplyBackdrop()
    {
        if (WindowBackdropType == OWindowBackdropType.None)
        {
            // 清除本地值，回落到样式的主题画刷（实底）
            ClearValue(BackgroundProperty);
            return;
        }

        // 透明化窗口背景，让 DWM 合成器材质透出
        SetValue(BackgroundProperty, Brushes.Transparent);
        WindowChromeInterop.SetBackdropType(this, (uint)WindowBackdropType);
    }

    private void OnThemeApplied(object? sender, AppTheme theme)
    {
        WindowChromeInterop.SetDarkMode(this, theme == AppTheme.Dark);
    }
}
