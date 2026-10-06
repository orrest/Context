namespace Plugin;

/// <summary>
/// 插件目录的设置条目：由插件目录从插件声明聚合而来。
/// <see cref="DefaultValue"/> 为目录聚合时的属性当前值（未被用户覆盖时生效）；
/// <see cref="SetValue"/> 回写用户值（null 表示清空，由提供者自行回落默认）。
/// </summary>
public sealed record SettingDeclaration(
    string Id,
    string DisplayName,
    string? DefaultValue,
    Action<string?> SetValue);
