namespace Plugin;

/// <summary>
/// 声明一个插件特有设置项：标注在命令提供者（<see cref="ICommandProvider"/> 实现）的
/// 公共可读写 string 属性上，由宿主的插件目录反射聚合。设置页据此渲染条目；
/// 用户值由宿主持久化（按 <see cref="Id"/> 存取），并经声明的属性写回即时生效。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PluginSettingAttribute(string id, string displayName) : Attribute
{
    /// <summary>设置项唯一标识（如 "music.cacheDirectory"），持久化按此存储。</summary>
    public string Id { get; } = id;

    /// <summary>展示名（设置页设置列表显示）。</summary>
    public string DisplayName { get; } = displayName;
}
