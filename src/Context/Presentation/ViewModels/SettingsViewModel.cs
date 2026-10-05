using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Plugin;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 设置窗口 ViewModel：主题、置顶与全局呼出热键偏好的加载、编辑与应用；「快捷键」页的条目生成。
/// 选项变更即持久化，并广播消息由主视图/快捷键服务应用。
/// </summary>
public partial class SettingsViewModel(
    IMessenger messenger,
    IAppSettingsRepository appSettingsRepository,
    IHotkeyCommandCatalog hotkeyCommandCatalog,
    IHotkeyBindingsRepository hotkeyBindingsRepository,
    Func<HotkeyItemViewModel> hotkeyItemFactory) : ObservableRecipient(messenger)
{
    /// <summary>应用主题名（Orrest.Ui AppTheme 枚举名），默认 null 表示未加载/未设置（应用侧回退浅色）。</summary>
    [ObservableProperty]
    public partial string? Theme { get; set; } = null;

    /// <summary>主窗口是否置顶，默认开启。</summary>
    [ObservableProperty]
    public partial bool IsTopmost { get; set; } = true;

    /// <summary>全局呼出快捷键手势（系统级注册），null/空表示停用。</summary>
    [ObservableProperty]
    public partial string? GlobalHotkey { get; set; } = null;

    /// <summary>「快捷键」页条目：命令目录 × 持久化绑定（未绑定用命令默认手势）。</summary>
    public ObservableCollection<HotkeyItemViewModel> Hotkeys { get; } = [];

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
        foreach (var command in hotkeyCommandCatalog.GetCommands())
        {
            var gesture = bound.TryGetValue(command.Id, out var binding)
                ? binding.KeyGesture
                : command.DefaultKeyGesture;
            var item = hotkeyItemFactory();
            item.Initialize(command.Id, command.DisplayName, gesture);
            Hotkeys.Add(item);
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
