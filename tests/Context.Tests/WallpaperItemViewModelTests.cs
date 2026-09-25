using Context.Domain.Entities;
using Context.Presentation.ViewModels;
using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperItemViewModelTests
{
    private const string ThumbnailUrl = "https://cn.bing.com/th?id=OHR.Test_UHD.jpg&w=384&h=216";
    private const string PreviewUrl = "https://cn.bing.com/th?id=OHR.Test_UHD.jpg";

    private readonly IWallpaperRepository _repository = Substitute.For<IWallpaperRepository>();
    private readonly IDesktopWallpaperSetter _setter = Substitute.For<IDesktopWallpaperSetter>();
    private int _thumbnailDownloadCount;
    private int _previewDownloadCount;
    private byte[][] _thumbnailResponses = [];
    private byte[][] _previewResponses = [];

    public WallpaperItemViewModelTests()
    {
        _repository.GetImageAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                if (string.Equals(callInfo.ArgAt<string>(0), ThumbnailUrl, StringComparison.Ordinal))
                {
                    return NextResponse(_thumbnailResponses, ref _thumbnailDownloadCount, "缩略图");
                }

                return NextResponse(_previewResponses, ref _previewDownloadCount, "预览图");
            });
    }

    private WallpaperItemViewModel CreateItem()
    {
        var wallpaper = new Wallpaper
        {
            Date = new DateOnly(2026, 9, 24),
            Title = "测试壁纸",
            ImageUrl = PreviewUrl,
            ThumbnailUrl = ThumbnailUrl,
        };
        return new WallpaperItemViewModel(wallpaper, new WallpaperService(_repository, _setter));
    }

    private void SetupThumbnailResponses(params byte[][] responses)
    {
        _thumbnailResponses = responses;
    }

    private void SetupPreviewResponses(params byte[][] responses)
    {
        _previewResponses = responses;
    }

    private static byte[] NextResponse(byte[][] responses, ref int count, string label)
    {
        if (responses.Length == 0)
        {
            throw new HttpRequestException($"未配置 {label} 的下载响应");
        }

        var index = Math.Min(count, responses.Length - 1);
        count++;
        var bytes = responses[index];
        if (bytes.Length == 0)
        {
            throw new HttpRequestException("网络失败");
        }

        return bytes;
    }

    [Fact]
    public async Task EnsureThumbnailLoadedAsync_DownloadsOnce_AndCaches()
    {
        SetupThumbnailResponses([1, 2, 3]);
        var item = CreateItem();

        var first = await item.EnsureThumbnailLoadedAsync();
        var second = await item.EnsureThumbnailLoadedAsync();

        Assert.Equal(new byte[] { 1, 2, 3 }, first);
        Assert.Equal(new byte[] { 1, 2, 3 }, second);
        Assert.Equal(1, _thumbnailDownloadCount);
        Assert.Equal(0, _previewDownloadCount);
    }

    [Fact]
    public async Task EnsurePreviewLoadedAsync_DownloadsPreviewUrl_AndCaches()
    {
        SetupPreviewResponses([7, 8]);
        var item = CreateItem();

        var first = await item.EnsurePreviewLoadedAsync();
        var second = await item.EnsurePreviewLoadedAsync();

        Assert.Equal(new byte[] { 7, 8 }, first);
        Assert.Equal(new byte[] { 7, 8 }, second);
        Assert.Equal(1, _previewDownloadCount);
        Assert.Equal(0, _thumbnailDownloadCount);
    }

    [Fact]
    public async Task EnsureThumbnailLoadedAsync_ReusesPreviewBytes_WithoutNetwork()
    {
        // 预览已缓存时，缩略图直接复用大图（视图层降采样），不访问网络
        SetupPreviewResponses([9, 9, 9]);
        var item = CreateItem();
        await item.EnsurePreviewLoadedAsync();

        var bytes = await item.EnsureThumbnailLoadedAsync();

        Assert.Equal(new byte[] { 9, 9, 9 }, bytes);
        Assert.Equal(new byte[] { 9, 9, 9 }, item.ThumbnailBytes);
        Assert.Equal(1, _previewDownloadCount);
        Assert.Equal(0, _thumbnailDownloadCount);
    }

    [Fact]
    public async Task EnsurePreviewLoadedAsync_Retries_WhenPreviousDownloadFailed()
    {
        // 第一次下载失败（空响应表示抛网络异常），第二次成功
        SetupPreviewResponses([], [4, 5]);
        var item = CreateItem();

        var failed = await item.EnsurePreviewLoadedAsync();
        var retried = await item.EnsurePreviewLoadedAsync();

        Assert.Null(failed);
        Assert.Equal(new byte[] { 4, 5 }, retried);
        Assert.Equal(2, _previewDownloadCount);
    }
}
