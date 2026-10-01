using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 设置窗口 ViewModel：主题与置顶偏好的加载、编辑与应用。
/// 选项变更即持久化，并广播 ThemeChangedMessage 由主视图应用主题。
/// </summary>
public partial class SettingsViewModel(
    IMessenger messenger,
    IAppSettingsRepository appSettingsRepository) : ObservableRecipient(messenger)
{
    /// <summary>应用主题名（Orrest.Ui AppTheme 枚举名），默认 null 表示未加载/未设置（应用侧回退浅色）。</summary>
    [ObservableProperty]
    public partial string? Theme { get; set; } = null;

    /// <summary>主窗口是否置顶，默认开启。</summary>
    [ObservableProperty]
    public partial bool IsTopmost { get; set; } = true;

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var settings = await appSettingsRepository.GetAsync(cancellationToken);
        Theme = settings.Theme;
        IsTopmost = settings.IsTopmost;
    }

    [RelayCommand]
    private async Task ApplyAsync(CancellationToken cancellationToken)
    {
        await appSettingsRepository.SaveAsync(
            new AppSettings { Theme = Theme, IsTopmost = IsTopmost }, cancellationToken);
        Messenger.Send(new SettingsAppliedMessage(Theme, IsTopmost));
    }
}
