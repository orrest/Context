using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.Presentation.Services;
using Plugin;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 设置窗口 ViewModel：主题、置顶与全局呼出热键偏好的加载、编辑与应用；「快捷键」页的
/// 快捷键条目与插件设置条目生成（均取自插件目录）。选项变更即持久化，并广播消息由主视图应用。
/// </summary>
public partial class SettingsViewModel(
    IMessenger messenger,
    IAppSettingsRepository appSettingsRepository,
    IPluginCatalog pluginCatalog,
    IHotkeyBindingsRepository hotkeyBindingsRepository,
    IPluginSettingsRepository pluginSettingsRepository,
    Func<HotkeyItemViewModel> hotkeyItemFactory,
    Func<SettingItemViewModel> settingItemFactory) : ObservableRecipient(messenger)
{
    /// <summary>应用主题名（Orrest.Ui AppTheme 枚举名），默认 null 表示未加载/未设置（应用侧回退浅色）。</summary>
    [ObservableProperty]
    public partial string? Theme { get; set; } = null;

    /// <summary>主窗口是否置顶，默认开启。</summary>
    [ObservableProperty]
    public partial bool IsTopmost { get; set; } = true;

    /// <summary>全局呼出快捷键手势（系统级注册，默认 "Alt+O"），null/空表示停用。</summary>
    [ObservableProperty]
    public partial string? GlobalHotkey { get; set; } = "Alt+O";

    /// <summary>「快捷键」条目：命令目录 × 持久化绑定（未绑定用命令默认手势）。</summary>
    public ObservableCollection<HotkeyItemViewModel> Hotkeys { get; } = [];

    /// <summary>「插件设置」条目：设置目录 × 持久化值（未设置用声明默认值）。</summary>
    public ObservableCollection<SettingItemViewModel> Settings { get; } = [];

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var settings = await appSettingsRepository.GetAsync(cancellationToken);
        Theme = settings.Theme;
        IsTopmost = settings.IsTopmost;
        GlobalHotkey = settings.GlobalHotkey;

        var bindings = await hotkeyBindingsRepository.GetAllAsync(cancellationToken);
        var bound = bindings.ToDictionary(binding => binding.CommandId);
        Hotkeys.Clear();
        foreach (var command in pluginCatalog.GetCommands())
        {
            var gesture = bound.TryGetValue(command.Id, out var binding)
                ? binding.KeyGesture
                : command.DefaultKeyGesture;
            var item = hotkeyItemFactory();
            item.Initialize(command.Id, command.DisplayName, gesture);
            Hotkeys.Add(item);
        }

        var persisted = await pluginSettingsRepository.GetAllAsync(cancellationToken);
        var values = persisted.ToDictionary(setting => setting.Id, setting => setting.Value);
        Settings.Clear();
        foreach (var declaration in pluginCatalog.GetSettings())
        {
            var value = values.TryGetValue(declaration.Id, out var setting)
                ? setting
                : declaration.DefaultValue ?? string.Empty;
            var item = settingItemFactory();
            item.Initialize(declaration.Id, declaration.DisplayName, value);
            Settings.Add(item);
        }
    }

    [RelayCommand]
    private async Task ApplyAsync(CancellationToken cancellationToken)
    {
        await appSettingsRepository.SaveAsync(
            new AppSettings
            {
                Theme = Theme,
                IsTopmost = IsTopmost,
                GlobalHotkey = GlobalHotkey,
            }, cancellationToken);
        Messenger.Send(new SettingsAppliedMessage(Theme, IsTopmost, GlobalHotkey));
    }
}
