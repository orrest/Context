using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// OWindow 样式几何度量常量。
/// 样式字典 <c>Styles/Window.xaml</c> 以 x:Static 引用；
/// 明暗配色在 <see cref="WindowThemeLight"/> / <see cref="WindowThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>Themes/Window.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class WindowTheme
{
    /// <summary>标题栏高度（与 OWindow 默认样式挂载的 WindowChrome.CaptionHeight 一致）。</summary>
    public static readonly double CaptionHeight = 32;

    /// <summary>标题栏按钮宽度（高度占满标题栏）。</summary>
    public static readonly double CaptionButtonWidth = 40;

    /// <summary>标题栏按钮图形字号（Segoe MDL2 Assets 字形）。</summary>
    public static readonly double GlyphFontSize = 10;

    /// <summary>标题文本的左边距。</summary>
    public static readonly Thickness TitleMargin = new(12, 0, 0, 0);
}

/// <summary>OWindow 浅色主题配色。</summary>
public static class WindowThemeLight
{
    /// <summary>窗口背景（DWM 背景未启用时的实底；启用时窗口背景为透明）。</summary>
    public static readonly Color Background = Color.FromRgb(0xFF, 0xFF, 0xFF);

    /// <summary>边框线。</summary>
    public static readonly Color Border = Color.FromRgb(0xE3, 0xE3, 0xE3);

    /// <summary>文本前景（经模板属性继承作用于窗口全部内容）。</summary>
    public static readonly Color Foreground = Color.FromRgb(0x1A, 0x1A, 0x1A);

    /// <summary>标题栏按钮悬停底。</summary>
    public static readonly Color Hover = Color.FromRgb(0xF4, 0xF4, 0xF4);

    /// <summary>标题栏按钮按下底。</summary>
    public static readonly Color Pressed = Color.FromRgb(0xE6, 0xE6, 0xE6);

    /// <summary>关闭按钮悬停底（Windows 惯例红，半透明以透出窗口材质）。</summary>
    public static readonly Color CloseHover = Color.FromArgb(0x80, 0xE8, 0x11, 0x23);

    /// <summary>关闭按钮按下底（比悬停更深一档）。</summary>
    public static readonly Color ClosePressed = Color.FromArgb(0xB3, 0xE8, 0x11, 0x23);
}

/// <summary>OWindow 暗色主题配色。</summary>
public static class WindowThemeDark
{
    /// <summary>窗口背景（DWM 背景未启用时的实底；启用时窗口背景为透明）。</summary>
    public static readonly Color Background = Color.FromRgb(0x2D, 0x2D, 0x2D);

    /// <summary>边框线。</summary>
    public static readonly Color Border = Color.FromRgb(0x40, 0x40, 0x40);

    /// <summary>文本前景（经模板属性继承作用于窗口全部内容）。</summary>
    public static readonly Color Foreground = Color.FromRgb(0xF2, 0xF2, 0xF2);

    /// <summary>标题栏按钮悬停底。</summary>
    public static readonly Color Hover = Color.FromRgb(0x3D, 0x3D, 0x3D);

    /// <summary>标题栏按钮按下底。</summary>
    public static readonly Color Pressed = Color.FromRgb(0x4A, 0x4A, 0x4A);

    /// <summary>关闭按钮悬停底（Windows 惯例红，半透明以透出窗口材质）。</summary>
    public static readonly Color CloseHover = Color.FromArgb(0x80, 0xE8, 0x11, 0x23);

    /// <summary>关闭按钮按下底（比悬停更深一档）。</summary>
    public static readonly Color ClosePressed = Color.FromArgb(0xB3, 0xE8, 0x11, 0x23);
}
