namespace Context.Domain.Entities;

/// <summary>插件特有设置项的持久化记录（plugin_settings：按 Id 整体覆盖，如 "music.cacheDirectory" → 目录串）。</summary>
public sealed record PluginSetting(string Id, string Value);
