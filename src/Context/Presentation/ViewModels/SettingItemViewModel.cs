using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Context.Domain.Entities;
using Context.Presentation.Services;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 设置页「功能」Tab 的插件设置条目：展示设置项并编辑其值，失焦/回车即持久化，
/// 并经 <see cref="PluginSettingsApplier"/> 回写提供者属性即时生效。
/// 清空值 = 删除持久化记录，回落到插件声明的默认值。
/// </summary>
public partial class SettingItemViewModel(
    IPluginSettingsRepository pluginSettingsRepository,
    PluginSettingsApplier settingsApplier) : ObservableObject
{
    /// <summary>设置项唯一标识（持久化按此存储）。</summary>
    [ObservableProperty]
    public partial string SettingId { get; private set; } = string.Empty;

    /// <summary>设置项展示名。</summary>
    [ObservableProperty]
    public partial string DisplayName { get; private set; } = string.Empty;

    /// <summary>当前生效值（未显式设置时为声明默认值；空白表示已清空、待回默认）。</summary>
    [ObservableProperty]
    public partial string Value { get; set; } = string.Empty;

    public void Initialize(string settingId, string displayName, string value)
    {
        SettingId = settingId;
        DisplayName = displayName;
        Value = value;
    }

    [RelayCommand]
    private async Task ApplySettingAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            await pluginSettingsRepository.DeleteAsync(SettingId, cancellationToken);
            settingsApplier.Apply(SettingId, null);
            return;
        }

        await pluginSettingsRepository.SaveAsync(
            new PluginSetting(SettingId, Value), cancellationToken);
        settingsApplier.Apply(SettingId, Value);
    }
}
