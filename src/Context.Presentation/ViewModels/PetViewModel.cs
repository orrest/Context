using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Presentation.Messages;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 桌宠 ViewModel：悬浮按钮交互的命令入口，组合壁纸模块完成"切换下一张桌面壁纸"。
/// 需要 Messenger 广播跨模块请求，故继承 ObservableRecipient。
/// </summary>
public partial class PetViewModel(
    IMessenger messenger,
    Func<WallpaperViewModel> wallpaperViewModelFactory) : ObservableRecipient(messenger)
{
    public WallpaperViewModel Wallpaper { get; } = wallpaperViewModelFactory();

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
}
