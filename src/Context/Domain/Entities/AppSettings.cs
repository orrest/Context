namespace Context.Domain.Entities;

/// <summary>应用设置：持久化在本地 SQLite 的用户偏好。</summary>
public sealed class AppSettings
{
    /// <summary>主窗口是否置顶，默认开启。</summary>
    public bool IsTopmost { get; set; } = true;

    /// <summary>应用主题名（对应 Orrest.Ui 的 AppTheme 枚举名），null 表示未设置（应用侧回退浅色）。</summary>
    public string? Theme { get; set; }
}
