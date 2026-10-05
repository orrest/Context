namespace Context.Presentation.Messages;

/// <summary>设置已应用（由设置面板发出，主视图接收并同步主题、置顶与全局热键）。</summary>
/// <param name="Theme">主题名（Orrest.Ui AppTheme 枚举名），null 表示未设置（应用侧回退浅色）。</param>
/// <param name="IsTopmost">主窗口是否置顶。</param>
/// <param name="GlobalHotkey">全局呼出快捷键手势，null/空表示停用。</param>
public sealed record SettingsAppliedMessage(string? Theme, bool IsTopmost, string? GlobalHotkey);
