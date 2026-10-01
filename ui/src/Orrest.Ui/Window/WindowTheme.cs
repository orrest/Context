using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// OWindow 样式几何度量常量。
/// 样式字典 <c>Window/Window.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 ContextMenu 共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// OWindow 独有的在 <see cref="WindowThemeLight"/> / <see cref="WindowThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>Window/Window.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
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

/// <summary>OWindow 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class WindowThemeLight
{
    /// <summary>标题栏按钮按下底。</summary>
    public static readonly Color Pressed = Color.FromRgb(0xE6, 0xE6, 0xE6);

    /// <summary>关闭按钮悬停底（Windows 惯例红，半透明以透出窗口材质）。</summary>
    public static readonly Color CloseHover = Color.FromArgb(0x80, 0xE8, 0x11, 0x23);

    /// <summary>关闭按钮按下底（比悬停更深一档）。</summary>
    public static readonly Color ClosePressed = Color.FromArgb(0xB3, 0xE8, 0x11, 0x23);
}

/// <summary>OWindow 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class WindowThemeDark
{
    /// <summary>标题栏按钮按下底。</summary>
    public static readonly Color Pressed = Color.FromRgb(0x4A, 0x4A, 0x4A);

    /// <summary>关闭按钮悬停底（Windows 惯例红，半透明以透出窗口材质）。</summary>
    public static readonly Color CloseHover = Color.FromArgb(0x80, 0xE8, 0x11, 0x23);

    /// <summary>关闭按钮按下底（比悬停更深一档）。</summary>
    public static readonly Color ClosePressed = Color.FromArgb(0xB3, 0xE8, 0x11, 0x23);
}
