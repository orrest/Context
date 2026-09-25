using Context.Domain.Entities;
using Context.UseCases.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Context.Presentation.ViewModels;

/// <summary>
/// 列表项 ViewModel：按尺寸变体独立缓存图片（缩略图 / 预览各自一份）。
/// 预览不能复用缩略图（会模糊）；缩略图可以复用已缓存的预览（大图降采样即可，省一次网络）。
/// </summary>
public partial class WallpaperItemViewModel(Wallpaper wallpaper, WallpaperService wallpaperService) : ObservableObject
{
    public Wallpaper Wallpaper { get; } = wallpaper;

    public string DateText
    {
        get { return Wallpaper.Date.ToString("yyyy-MM-dd"); }
    }

    public string Title
    {
        get { return Wallpaper.Title; }
    }

    /// <summary>缩略图内容（异步加载后填充，由视图层解码显示）。</summary>
    [ObservableProperty]
    public partial byte[]? ThumbnailBytes { get; set; }

    /// <summary>预览图内容。</summary>
    [ObservableProperty]
    public partial byte[]? PreviewBytes { get; set; }

    /// <summary>确保缩略图已加载：已缓存直接返回；预览已缓存则复用（降采样显示），否则下载缩略图。</summary>
    public async Task<byte[]?> EnsureThumbnailLoadedAsync()
    {
        if (ThumbnailBytes is { Length: > 0 })
        {
            return ThumbnailBytes;
        }

        if (PreviewBytes is { Length: > 0 })
        {
            ThumbnailBytes = PreviewBytes;
            return ThumbnailBytes;
        }

        ThumbnailBytes = await DownloadAsync(Wallpaper.ThumbnailUrl);
        return ThumbnailBytes;
    }

    /// <summary>确保预览图已加载：已缓存直接返回，不访问网络。</summary>
    public async Task<byte[]?> EnsurePreviewLoadedAsync()
    {
        if (PreviewBytes is { Length: > 0 })
        {
            return PreviewBytes;
        }

        PreviewBytes = await DownloadAsync(Wallpaper.ImageUrl);
        return PreviewBytes;
    }

    private async Task<byte[]?> DownloadAsync(string url)
    {
        try
        {
            return await wallpaperService.GetImageAsync(url);
        }
        catch
        {
            // 下载失败保持缓存为空，下次加载可重试
            return null;
        }
    }
}
