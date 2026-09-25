using System.Collections.ObjectModel;
using Context.Domain.Entities;
using Context.UseCases.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 壁纸模块 ViewModel：加载最近壁纸，并支持"切换到下一张并设为桌面壁纸"。
/// </summary>
public partial class WallpaperViewModel(WallpaperService wallpaperService) : ObservableObject
{
    private const int RecentDays = 8; // Bing 归档接口最多返回最近 8 天
    private int _currentIndex = -1;

    public ObservableCollection<Wallpaper> Wallpapers { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy
    {
        get { return !IsBusy; }
    }

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        try
        {
            await EnsureLoadedAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>切换到下一张并设为桌面壁纸：首次为今天，到末尾后循环；列表按需懒加载。</summary>
    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task SetNextAsDesktopAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await EnsureLoadedAsync(cancellationToken);
            if (Wallpapers.Count == 0)
            {
                return;
            }

            _currentIndex = (_currentIndex + 1) % Wallpapers.Count;
            await wallpaperService.SetAsDesktopAsync(Wallpapers[_currentIndex], cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (Wallpapers.Count > 0)
        {
            return;
        }

        foreach (var wallpaper in await wallpaperService.GetRecentAsync(RecentDays, cancellationToken))
        {
            Wallpapers.Add(wallpaper);
        }
    }
}
