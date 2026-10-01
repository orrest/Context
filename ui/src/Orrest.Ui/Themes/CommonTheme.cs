using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// ContextMenu 与 Window 共用的配色常量(语义与数值一致的属性)。
/// 主题字典 <c>ContextMenu/ContextMenu.Light|Dark.xaml</c> 与 <c>Window/Window.Light|Dark.xaml</c> 以 x:Static 引用;
/// 明暗配色在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>
/// (XAML 的 x:Static 不支持嵌套类，故拆为顶层类)。
/// </summary>
public static class CommonThemeLight
{
    /// <summary>背景（不透明基色，透明度由样式背景层绑定 Theme.BackgroundOpacity 实现）。</summary>
    public static readonly Color Background = Color.FromRgb(0xFF, 0xFF, 0xFF);

    /// <summary>边框线。</summary>
    public static readonly Color Border = Color.FromRgb(0xE3, 0xE3, 0xE3);

    /// <summary>文本前景。</summary>
    public static readonly Color Foreground = Color.FromRgb(0x1A, 0x1A, 0x1A);

    /// <summary>悬停底。</summary>
    public static readonly Color Hover = Color.FromRgb(0xF4, 0xF4, 0xF4);
}

/// <summary>ContextMenu 与 Window 共用的暗色配色。</summary>
public static class CommonThemeDark
{
    /// <summary>背景（不透明基色，透明度由样式背景层绑定 Theme.BackgroundOpacity 实现）。</summary>
    public static readonly Color Background = Color.FromRgb(0x2D, 0x2D, 0x2D);

    /// <summary>边框线。</summary>
    public static readonly Color Border = Color.FromRgb(0x40, 0x40, 0x40);

    /// <summary>文本前景。</summary>
    public static readonly Color Foreground = Color.FromRgb(0xF2, 0xF2, 0xF2);

    /// <summary>悬停底。</summary>
    public static readonly Color Hover = Color.FromRgb(0x3D, 0x3D, 0x3D);
}
