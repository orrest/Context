using System.Windows;

namespace Orrest.Ui.Themes;

/// <summary>应用主题模式。命名避开 WPF 自带的 Application/Window.ThemeMode 属性。</summary>
public enum AppTheme
{
    /// <summary>浅色主题。</summary>
    Light,

    /// <summary>暗色主题。</summary>
    Dark,
}

/// <summary>
/// 通用主题切换器：运行时替换应用级合并的总主题字典（<c>Theme.Light.xaml</c> / <c>Theme.Dark.xaml</c>，
/// 各自合并全部组件主题字典），并提供背景透明度的外部设置——组件样式的背景层经
/// DynamicResource 绑定 <c>Theme.BackgroundOpacity</c> 资源（0–1），本类写入该资源即全局生效。
/// </summary>
public static class ThemeManager
{
    private const string BackgroundOpacityKey = "Theme.BackgroundOpacity";

    private static readonly ResourceDictionary LightTheme = new()
    {
        Source = new Uri("pack://application:,,,/Orrest.Ui;component/Themes/Theme.Light.xaml", UriKind.Absolute),
    };

    private static readonly ResourceDictionary DarkTheme = new()
    {
        Source = new Uri("pack://application:,,,/Orrest.Ui;component/Themes/Theme.Dark.xaml", UriKind.Absolute),
    };

    private static ResourceDictionary? _current;

    /// <summary>当前应用的主题模式；尚未应用过任何主题时为 <see cref="AppTheme.Light"/>。</summary>
    public static AppTheme Current { get; private set; } = AppTheme.Light;

    /// <summary>主题切换完成（<see cref="Apply"/> 替换字典后触发）。</summary>
    public static event EventHandler<AppTheme>? ThemeApplied;

    /// <summary>切换明暗主题：移除当前总主题字典并插入目标主题。</summary>
    public static void Apply(AppTheme mode)
    {
        var theme = mode == AppTheme.Dark ? DarkTheme : LightTheme;
        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
        if (_current is not null)
        {
            mergedDictionaries.Remove(_current);
        }

        mergedDictionaries.Insert(0, theme);
        _current = theme;
        Current = mode;
        ThemeApplied?.Invoke(null, mode);
    }

    /// <summary>
    /// 设置背景透明度（0–1 的不透明度，超出范围按边界截断）。
    /// 所有组件样式的背景层经 DynamicResource 绑定该值，运行时即时生效。
    /// </summary>
    public static void SetBackgroundOpacity(double opacity)
    {
        if (opacity < 0)
        {
            opacity = 0;
        }

        if (opacity > 1)
        {
            opacity = 1;
        }

        Application.Current.Resources[BackgroundOpacityKey] = opacity;
    }
}
