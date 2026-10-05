using Orrest.Ui.Themes;

namespace Context.App;

/// <summary>
/// 主题名（字符串）到 Orrest.Ui 主题枚举的映射。
/// 领域层以字符串存储主题，UI 层使用 Orrest.Ui.Themes.AppTheme 枚举，映射仅此一处。
/// </summary>
internal static class ThemeMapper
{
    public static AppTheme Map(string? theme)
    {
        return Enum.TryParse(theme, ignoreCase: true, out AppTheme mode)
            ? mode
            : AppTheme.Light;
    }
}
