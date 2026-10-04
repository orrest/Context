using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// ComboBox 样式几何度量常量。
/// 样式字典 <c>ComboBox/ComboBox.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 ContextMenu / Window 共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// ComboBox 独有的在 <see cref="ComboBoxThemeLight"/> / <see cref="ComboBoxThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>ComboBox/ComboBox.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class ComboBoxTheme
{
    /// <summary>收起状态框的最小高度。</summary>
    public static readonly double BoxMinHeight = 28;

    /// <summary>收起状态框圆角半径。</summary>
    public static readonly CornerRadius BoxCornerRadius = new(5);

    /// <summary>收起状态框边框线宽。</summary>
    public static readonly Thickness BoxBorderThickness = new(1);

    /// <summary>框内内容区与边框线的内边距。</summary>
    public static readonly Thickness BoxPadding = new(10, 0, 10, 0);

    /// <summary>下拉箭头（chevron）图形尺寸。</summary>
    public static readonly double ChevronWidth = 11;

    /// <summary>下拉箭头（chevron）图形高度。</summary>
    public static readonly double ChevronHeight = 7;

    /// <summary>下拉弹出层外框圆角半径（与 ContextMenu 菜单容器一致）。</summary>
    public static readonly CornerRadius DropdownCornerRadius = new(8);

    /// <summary>下拉弹出层边框线宽。</summary>
    public static readonly Thickness DropdownBorderThickness = new(1);

    /// <summary>下拉列表内边距（边缘留白）。</summary>
    public static readonly Thickness DropdownPadding = new(6);

    /// <summary>下拉项悬停高亮底的圆角半径。</summary>
    public static readonly CornerRadius ItemCornerRadius = new(5);

    /// <summary>下拉项外边距（与弹出层边缘的间隙）。</summary>
    public static readonly Thickness ItemMargin = new(2, 1, 2, 1);

    /// <summary>下拉项内边距。</summary>
    public static readonly Thickness ItemPadding = new(10, 6, 10, 6);
}

/// <summary>ComboBox 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class ComboBoxThemeLight
{
    /// <summary>悬停时的边框色（比共用 Border 加深）。</summary>
    public static readonly Color HoverBorder = Color.FromRgb(0xC2, 0xC2, 0xC2);

    /// <summary>聚焦/展开时的边框色（强调色，与 ContextMenu 勾选标记同源）。</summary>
    public static readonly Color FocusedBorder = Color.FromRgb(0x6E, 0x56, 0xCF);

    /// <summary>选中项的勾选标记（强调色）。</summary>
    public static readonly Color Glyph = Color.FromRgb(0x6E, 0x56, 0xCF);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0xA6, 0xA6, 0xA6);
}

/// <summary>ComboBox 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class ComboBoxThemeDark
{
    /// <summary>悬停时的边框色（比共用 Border 提亮）。</summary>
    public static readonly Color HoverBorder = Color.FromRgb(0x6E, 0x6E, 0x6E);

    /// <summary>聚焦/展开时的边框色（强调色，与 ContextMenu 勾选标记同源）。</summary>
    public static readonly Color FocusedBorder = Color.FromRgb(0xA7, 0x8B, 0xFA);

    /// <summary>选中项的勾选标记（强调色）。</summary>
    public static readonly Color Glyph = Color.FromRgb(0xA7, 0x8B, 0xFA);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0x7C, 0x7C, 0x7C);
}
