using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// CheckBox 样式几何度量常量。
/// 样式字典 <c>CheckBox/CheckBox.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 Window 共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// CheckBox 独有的在 <see cref="CheckBoxThemeLight"/> / <see cref="CheckBoxThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>CheckBox/CheckBox.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class CheckBoxTheme
{
    /// <summary>勾选框边长（正方形）。</summary>
    public static readonly double BoxSize = 16;

    /// <summary>勾选框圆角半径。</summary>
    public static readonly CornerRadius BoxCornerRadius = new(4);

    /// <summary>勾选框边框线宽。</summary>
    public static readonly Thickness BoxBorderThickness = new(1);

    /// <summary>勾选框与内容文本的间距。</summary>
    public static readonly Thickness ContentMargin = new(8, 0, 0, 0);

    /// <summary>禁用时勾选框整体的不透明度（与总主题字典的背景层透明度无关）。</summary>
    public static readonly double DisabledOpacity = 0.4;
}

/// <summary>CheckBox 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class CheckBoxThemeLight
{
    /// <summary>勾选后的勾选框底色（强调色，与 ContextMenu 勾选标记一致）。</summary>
    public static readonly Color Accent = Color.FromRgb(0x6E, 0x56, 0xCF);

    /// <summary>悬停时的勾选框底色（强调色加深）。</summary>
    public static readonly Color AccentHover = Color.FromRgb(0x65, 0x4D, 0xC4);

    /// <summary>勾选框内的对勾/短横线。</summary>
    public static readonly Color Checkmark = Color.FromRgb(0xFF, 0xFF, 0xFF);

    /// <summary>未勾选悬停时的边框色（比共用 Border 加深）。</summary>
    public static readonly Color HoverBorder = Color.FromRgb(0xC2, 0xC2, 0xC2);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0xA6, 0xA6, 0xA6);
}

/// <summary>CheckBox 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class CheckBoxThemeDark
{
    /// <summary>勾选后的勾选框底色（强调色，与 ContextMenu 勾选标记一致）。</summary>
    public static readonly Color Accent = Color.FromRgb(0xA7, 0x8B, 0xFA);

    /// <summary>悬停时的勾选框底色（强调色提亮）。</summary>
    public static readonly Color AccentHover = Color.FromRgb(0xC4, 0xB5, 0xFD);

    /// <summary>勾选框内的对勾/短横线（深色，压在浅色强调底上）。</summary>
    public static readonly Color Checkmark = Color.FromRgb(0x2D, 0x2D, 0x2D);

    /// <summary>未勾选悬停时的边框色（比共用 Border 提亮）。</summary>
    public static readonly Color HoverBorder = Color.FromRgb(0x6E, 0x6E, 0x6E);

    /// <summary>禁用文本。</summary>
    public static readonly Color DisabledForeground = Color.FromRgb(0x7C, 0x7C, 0x7C);
}
