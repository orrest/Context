namespace Plugin;

/// <summary>
/// 声明一个可经快捷键绑定的命令：标注在命令提供者（<see cref="ICommandProvider"/> 实现）
/// 的公共 <see cref="System.Windows.Input.ICommand"/> 属性上，由宿主的命令目录反射聚合。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class HotkeyCommandAttribute(string id, string displayName, string defaultKeyGesture)
    : Attribute
{
    /// <summary>命令唯一标识（如 "wallpaper.next"），快捷键绑定按此存储。</summary>
    public string Id { get; } = id;

    /// <summary>展示名（设置页快捷键列表显示）。</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>默认键手势（如 "F8"、"Ctrl+Shift+W"），未被用户覆盖时生效。</summary>
    public string DefaultKeyGesture { get; } = defaultKeyGesture;
}
