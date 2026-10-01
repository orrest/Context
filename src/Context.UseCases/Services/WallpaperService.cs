using Context.Domain.Entities;
using Context.UseCases.Abstractions;

namespace Context.UseCases.Services;

public sealed class WallpaperService(IWallpaperRepository repository, IDesktopWallpaperSetter wallpaperSetter)
{
    private const int RecentDays = 8; // Bing 归档接口最多返回最近 8 天
    private int _currentIndex = -1;
    private IReadOnlyList<Wallpaper>? _recent;

    public async Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        return await repository.GetImageAsync(url, cancellationToken);
    }

    /// <summary>切换到下一张并设为桌面壁纸：首次为今天，到末尾后循环；列表按需懒加载。</summary>
    public async Task SetNextAsDesktopAsync(CancellationToken cancellationToken = default)
    {
        if (_recent is null)
        {
            var recent = await repository.GetRecentAsync(RecentDays, cancellationToken);
            if (recent.Count == 0)
            {
                return; // 空列表不缓存，下次调用重试
            }

            _recent = recent;
        }

        _currentIndex = (_currentIndex + 1) % _recent.Count;
        await SetAsDesktopAsync(_recent[_currentIndex], cancellationToken);
    }

    private async Task SetAsDesktopAsync(Wallpaper wallpaper, CancellationToken cancellationToken)
    {
        var imageBytes = await repository.GetImageAsync(wallpaper.ImageUrl, cancellationToken);
        await wallpaperSetter.SetAsync(imageBytes, $"{wallpaper.Date:yyyy-MM-dd}.jpg", cancellationToken);
    }
}
