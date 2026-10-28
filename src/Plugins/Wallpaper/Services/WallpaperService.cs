using Wallpaper.Abstractions;
using WallpaperEntity = Wallpaper.Entities.Wallpaper;

namespace Wallpaper.Services;

public sealed class WallpaperService(IWallpaperRepository repository, IDesktopWallpaperSetter wallpaperSetter)
{
    private const int RecentDays = 8; // Bing 归档接口最多返回最近 8 天
    private int _currentIndex = -1;
    private IReadOnlyList<WallpaperEntity>? _recent;

    public async Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        return await repository.GetImageAsync(url, cancellationToken);
    }

    /// <summary>切换到下一张并设为桌面壁纸：首次为今天，到末尾后循环；列表按需懒加载，跨天后自动刷新。</summary>
    public async Task SetNextAsDesktopAsync(CancellationToken cancellationToken = default)
    {
        // The cached list stays valid only while its newest wallpaper is dated today.
        // 缓存列表仅当其最新壁纸日期为今天时有效，跨天后重新拉取。
        if (_recent is null || _recent[0].Date < DateOnly.FromDateTime(DateTime.Now))
        {
            var recent = await repository.GetRecentAsync(RecentDays, cancellationToken);
            if (recent.Count == 0)
            {
                // Empty list is not cached; the next call retries.
                // 空列表不缓存，下次调用重试。
                return;
            }

            // Keep the browse position while the archive still ends on the same day; restart from today once a new day appears.
            // 归档最新一天未变时保持浏览进度；出现新一天的壁纸则从头开始。
            if (_recent is null || recent[0].Date != _recent[0].Date)
            {
                _currentIndex = -1;
            }

            _recent = recent;
        }

        _currentIndex = (_currentIndex + 1) % _recent.Count;
        await SetAsDesktopAsync(_recent[_currentIndex], cancellationToken);
    }

    private async Task SetAsDesktopAsync(WallpaperEntity wallpaper, CancellationToken cancellationToken)
    {
        var imageBytes = await repository.GetImageAsync(wallpaper.ImageUrl, cancellationToken);
        await wallpaperSetter.SetAsync(imageBytes, $"{wallpaper.Date:yyyy-MM-dd}.jpg", cancellationToken);
    }
}
