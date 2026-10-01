using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Domain.Entities;
using Context.Presentation.Messages;
using Context.UseCases.Abstractions;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 桌宠 ViewModel：右键菜单的命令入口，组合壁纸模块完成"切换下一张桌面壁纸"，
/// 并经应用设置仓储维护"置顶"偏好（默认置顶）。
/// 需要 Messenger 广播跨模块请求，故继承 ObservableRecipient。
/// </summary>
public partial class PetViewModel(
    IMessenger messenger,
    Func<WallpaperViewModel> wallpaperViewModelFactory,
    IAppSettingsRepository appSettingsRepository) : ObservableRecipient(messenger)
{
    public WallpaperViewModel Wallpaper { get; } = wallpaperViewModelFactory();

    /// <summary>主窗口是否置顶，默认 true；由视图 Loaded 触发的初始化命令从设置库加载。</summary>
    [ObservableProperty]
    public partial bool IsTopmost { get; set; } = true;

    [RelayCommand]
    private async Task NextWallpaperAsync(CancellationToken cancellationToken)
    {
        await Wallpaper.SetNextAsDesktopCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void OpenDictionary()
    {
        // 跨模块 UI 请求经 Messenger 广播，由主视图接收并弹出词典窗口
        Messenger.Send(new DictionaryRequestedMessage());
    }

    [RelayCommand]
    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var settings = await appSettingsRepository.GetAsync(cancellationToken);
        IsTopmost = settings.IsTopmost;
    }

    [RelayCommand]
    private async Task ToggleTopmostAsync(CancellationToken cancellationToken)
    {
        IsTopmost = !IsTopmost;
        await appSettingsRepository.SaveAsync(
            new AppSettings { IsTopmost = IsTopmost }, cancellationToken);
    }
}
