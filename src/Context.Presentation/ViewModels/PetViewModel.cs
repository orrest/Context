using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Presentation.Messages;
using Context.UseCases.Services;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 桌宠 ViewModel：右键菜单的命令入口，壁纸切换直接委托 WallpaperService。
/// 主题与置顶偏好由设置窗口（SettingsViewModel）管理。
/// 需要 Messenger 广播跨模块请求，故继承 ObservableRecipient。
/// </summary>
public partial class PetViewModel(IMessenger messenger, WallpaperService wallpaperService) :
    ObservableRecipient(messenger)
{
    [RelayCommand]
    private async Task NextWallpaperAsync(CancellationToken cancellationToken)
    {
        await wallpaperService.SetNextAsDesktopAsync(cancellationToken);
    }

    [RelayCommand]
    private void OpenDictionary()
    {
        // 跨模块 UI 请求经 Messenger 广播，由主视图接收并弹出词典窗口
        Messenger.Send(new DictionaryRequestedMessage());
    }

    [RelayCommand]
    private void OpenSettings()
    {
        Messenger.Send(new SettingsRequestedMessage());
    }
}
