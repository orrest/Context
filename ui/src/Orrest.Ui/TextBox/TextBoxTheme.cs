using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// TextBox 样式几何度量常量。
/// 样式字典 <c>TextBox/TextBox.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 ContextMenu/Window 共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// TextBox 独有的在 <see cref="TextBoxThemeLight"/> / <see cref="TextBoxThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>TextBox/TextBox.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class TextBoxTheme
{
    /// <summary>文本框圆角半径。</summary>
    public static readonly CornerRadius CornerRadius = new(4);

    /// <summary>文本框边框线宽。</summary>
    public static readonly Thickness BorderThickness = new(1);

    /// <summary>文本内边距默认值（使用处可用局部值覆盖）。</summary>
    public static readonly Thickness Padding = new(6, 4, 6, 4);
}

/// <summary>TextBox 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class TextBoxThemeLight
{
    /// <summary>聚焦边框（强调色，与 ContextMenu 勾选标记同源）。</summary>
    public static readonly Color FocusedBorder = Color.FromRgb(0x6E, 0x56, 0xCF);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0xA6, 0xA6, 0xA6);
}

/// <summary>TextBox 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class TextBoxThemeDark
{
    /// <summary>聚焦边框（强调色，与 ContextMenu 勾选标记同源）。</summary>
    public static readonly Color FocusedBorder = Color.FromRgb(0xA7, 0x8B, 0xFA);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0x7C, 0x7C, 0x7C);
}
