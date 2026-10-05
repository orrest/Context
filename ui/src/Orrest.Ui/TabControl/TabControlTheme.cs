using System.Windows;
using System.Windows.Media;

namespace Orrest.Ui.Themes;

/// <summary>
/// TabControl / TabItem 样式几何度量常量。
/// 样式字典 <c>TabControl/TabControl.xaml</c> 以 x:Static 引用；
/// 明暗配色：与 ContextMenu 等共用的部分在 <see cref="CommonThemeLight"/> / <see cref="CommonThemeDark"/>，
/// TabControl 独有的在 <see cref="TabControlThemeLight"/> / <see cref="TabControlThemeDark"/>
/// （XAML 的 x:Static 不支持嵌套类，故拆为顶层类），
/// 主题字典 <c>TabControl/TabControl.Light|Dark.xaml</c> 取色映射为画刷资源，由应用合并其一完成主题选择。
/// </summary>
public static class TabControlTheme
{
    /// <summary>标签页头部与内容的内边距。</summary>
    public static readonly Thickness TabPadding = new(12, 6, 12, 6);

    /// <summary>选中标签下方指示条的高度。</summary>
    public static readonly double UnderlineHeight = 2;

    /// <summary>指示条与标签两端的水平收缩（类型须为 Thickness：x:Static 不做类型转换，误用 double 会在测量阶段拆箱失败）。</summary>
    public static readonly Thickness UnderlineMargin = new(4, 0, 4, 0);

    /// <summary>禁用时标签整体的不透明度。</summary>
    public static readonly double DisabledOpacity = 0.4;
}

/// <summary>TabControl 独有浅色配色；共用色见 <see cref="CommonThemeLight"/>。</summary>
public static class TabControlThemeLight
{
    /// <summary>选中标签指示条（强调色，与 CheckBox 勾选一致）。</summary>
    public static readonly Color Accent = Color.FromRgb(0x6E, 0x56, 0xCF);
}

/// <summary>TabControl 独有暗色配色；共用色见 <see cref="CommonThemeDark"/>。</summary>
public static class TabControlThemeDark
{
    /// <summary>选中标签指示条（强调色，与 CheckBox 勾选一致）。</summary>
    public static readonly Color Accent = Color.FromRgb(0xA7, 0x8B, 0xFA);
}
