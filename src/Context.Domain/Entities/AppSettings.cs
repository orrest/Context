namespace Context.Domain.Entities;

/// <summary>应用设置：持久化在本地 SQLite 的用户偏好。</summary>
public sealed class AppSettings
{
    /// <summary>主窗口是否置顶，默认开启。</summary>
    public bool IsTopmost { get; set; } = true;
}
