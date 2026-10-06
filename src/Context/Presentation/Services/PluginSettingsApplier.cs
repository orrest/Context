using Context.UseCases.Abstractions;
using Plugin;

namespace Context.Presentation.Services;

/// <summary>
/// 插件设置应用器：启动时把持久化的插件设置逐一回写到目录声明（声明 SetValue），
/// 设置页保存后按 Id 即时应用——提供者属性即刻反映用户值，无需重启或消息广播。
/// </summary>
public sealed class PluginSettingsApplier(IPluginCatalog pluginCatalog, IPluginSettingsRepository settingsRepository)
{
    /// <summary>启动装配：把全部持久化设置回写到对应声明（声明缺失时忽略该条）。</summary>
    public async Task ApplyAllAsync(CancellationToken cancellationToken = default)
    {
        var declarations = pluginCatalog.GetSettings().ToDictionary(declaration => declaration.Id);
        foreach (var setting in await settingsRepository.GetAllAsync(cancellationToken))
        {
            if (declarations.TryGetValue(setting.Id, out var declaration))
            {
                declaration.SetValue(setting.Value);
            }
        }
    }

    /// <summary>单条应用：value 为 null（清空）时回落到声明默认值。</summary>
    public void Apply(string id, string? value)
    {
        var declaration = pluginCatalog.GetSettings().FirstOrDefault(item => item.Id == id);
        if (declaration is null)
        {
            return;
        }

        declaration.SetValue(value ?? declaration.DefaultValue);
    }
}
