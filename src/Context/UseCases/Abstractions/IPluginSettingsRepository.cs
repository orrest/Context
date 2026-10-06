using Context.Domain.Entities;

namespace Context.UseCases.Abstractions;

public interface IPluginSettingsRepository
{
    /// <summary>读取全部插件设置；未配置项视为采用插件声明的默认值。</summary>
    Task<IReadOnlyList<PluginSetting>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>保存设置（按 Id 整体覆盖）。</summary>
    Task SaveAsync(PluginSetting setting, CancellationToken cancellationToken = default);

    /// <summary>删除设置（该项回退到插件声明的默认值）。</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
