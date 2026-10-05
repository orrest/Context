namespace Context.Presentation.Messages;

/// <summary>设置已应用（由设置面板发出，主视图接收并同步主题与置顶状态）。</summary>
/// <param name="Theme">主题名（Orrest.Ui AppTheme 枚举名），null 表示未设置（应用侧回退浅色）。</param>
/// <param name="IsTopmost">主窗口是否置顶。</param>
public sealed record SettingsAppliedMessage(string? Theme, bool IsTopmost);
