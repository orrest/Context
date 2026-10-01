using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// ContextMenu 样式几何度量常量。
/// 样式字典 <c>ContextMenu/ContextMenu.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 Window 共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// ContextMenu 独有的在 <see cref="ContextMenuThemeLight"/> / <see cref="ContextMenuThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>ContextMenu/ContextMenu.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class ContextMenuTheme
{
    /// <summary>菜单外框圆角半径。</summary>
    public static readonly CornerRadius MenuCornerRadius = new(8);

    /// <summary>菜单外框边框线宽。</summary>
    public static readonly Thickness MenuBorderThickness = new(1);

    /// <summary>菜单内边距（边缘留白）。</summary>
    public static readonly Thickness MenuPadding = new(6);

    /// <summary>菜单项悬停高亮底的圆角半径。</summary>
    public static readonly CornerRadius ItemCornerRadius = new(5);

    /// <summary>菜单项外边距（与菜单边缘的间隙）。</summary>
    public static readonly Thickness ItemMargin = new(2, 1, 2, 1);

    /// <summary>菜单项内边距。</summary>
    public static readonly Thickness ItemPadding = new(10, 6, 10, 6);

    /// <summary>菜单项勾选/箭头列宽度（保证各菜单项文本对齐）。</summary>
    public static readonly GridLength GlyphColumnWidth = new(22);

    /// <summary>分隔线的水平留白与上下间距。</summary>
    public static readonly Thickness SeparatorMargin = new(6, 4, 6, 4);
}

/// <summary>ContextMenu 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class ContextMenuThemeLight
{
    /// <summary>分隔线。</summary>
    public static readonly Color Separator = Color.FromRgb(0xEC, 0xEC, 0xEC);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0xA6, 0xA6, 0xA6);

    /// <summary>勾选标记（强调色）。</summary>
    public static readonly Color Glyph = Color.FromRgb(0x00, 0x67, 0xC0);
}

/// <summary>ContextMenu 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class ContextMenuThemeDark
{
    /// <summary>分隔线。</summary>
    public static readonly Color Separator = Color.FromRgb(0x47, 0x47, 0x47);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0x7C, 0x7C, 0x7C);

    /// <summary>勾选标记（强调色）。</summary>
    public static readonly Color Glyph = Color.FromRgb(0x60, 0xCD, 0xFF);
}
