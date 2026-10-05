namespace Context.Domain.Entities;

/// <summary>用户为某命令显式配置的键手势绑定（command_id → 手势串，如 "wallpaper.next" → "Ctrl+F8"）。</summary>
public sealed record HotkeyBinding(string CommandId, string KeyGesture);
