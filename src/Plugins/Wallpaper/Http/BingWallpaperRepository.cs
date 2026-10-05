using System.Text.Json;
using Wallpaper.Abstractions;
using WallpaperEntity = Wallpaper.Entities.Wallpaper;

namespace Wallpaper.Http;

/// <summary>
/// 通过必应归档接口获取每日壁纸（机制参考 niumoo/bing-wallpaper）：
/// GET {base}/HPImageArchive.aspx?format=js&idx=0&n={days}
/// 返回 images[]。urlbase 拼接 "_UHD.jpg" 即 4K 原图；url 字段为 1080p 变体；
/// 均支持追加 w/h 参数由 Bing CDN 缩放（缩略图）。
/// </summary>
public sealed class BingWallpaperRepository(HttpClient httpClient) : IWallpaperRepository
{
    private const int MaxDays = 8; // 归档接口最多回溯 8 天

    public async Task<IReadOnlyList<WallpaperEntity>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
    {
        days = Math.Clamp(days, 1, MaxDays);
        var json = await httpClient.GetStringAsync(
            $"HPImageArchive.aspx?format=js&idx=0&n={days}", cancellationToken);

        using var document = JsonDocument.Parse(json);
        var images = document.RootElement.GetProperty("images");

        var wallpapers = new List<WallpaperEntity>();
        foreach (var image in images.EnumerateArray())
        {
            var path = image.GetProperty("url").GetString();
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            // urlbase 不带尺寸后缀（/th?id=OHR....），是 UHD 变体的公共前缀
            var urlBase = image.TryGetProperty("urlbase", out var urlBaseElement)
                ? urlBaseElement.GetString()
                : null;

            wallpapers.Add(new WallpaperEntity
            {
                Date = DateOnly.ParseExact(image.GetProperty("startdate").GetString()!, "yyyyMMdd"),
                Title = image.GetProperty("copyright").GetString() ?? string.Empty,
                ImageUrl = string.IsNullOrEmpty(urlBase)
                    ? AppendSize(path, "w=1920&h=1080")
                    : AppendSize($"{urlBase}_UHD.jpg", "w=3840&h=2160&rs=1&c=4"),
                ThumbnailUrl = AppendSize(path, "w=384&h=216"),
            });
        }

        return wallpapers;
    }

    public async Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetByteArrayAsync(url, cancellationToken);
    }

    /// <summary>path 已含查询串（th?id=...），直接追加尺寸参数。</summary>
    private static string AppendSize(string path, string size)
    {
        return $"{path}&{size}";
    }
}
