using Context.Domain.Entities;

namespace Context.UseCases.Abstractions;

public interface IWallpaperRepository
{
    /// <summary>获取最近 N 天的每日壁纸（最多 8 天，Bing 归档接口的上限）。</summary>
    Task<IReadOnlyList<Wallpaper>> GetRecentAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>按 URL 下载壁纸图片内容。</summary>
    Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default);
}
