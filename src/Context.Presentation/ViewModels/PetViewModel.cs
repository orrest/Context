using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 桌宠 ViewModel：悬浮按钮交互的命令入口，组合壁纸模块完成"切换下一张桌面壁纸"。
/// </summary>
public partial class PetViewModel(WallpaperViewModel wallpaper) : ObservableObject
{
    public WallpaperViewModel Wallpaper { get; } = wallpaper;

    [RelayCommand]
    private async Task NextWallpaperAsync(CancellationToken cancellationToken)
    {
        await Wallpaper.SetNextAsDesktopCommand.ExecuteAsync(null);
    }
}
