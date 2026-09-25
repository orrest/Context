using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperServiceTests
{
    private readonly IWallpaperRepository _repository = Substitute.For<IWallpaperRepository>();
    private readonly IDesktopWallpaperSetter _setter = Substitute.For<IDesktopWallpaperSetter>();
    private readonly WallpaperService _service;

    public WallpaperServiceTests()
    {
        _service = new WallpaperService(_repository, _setter);
    }

    [Fact]
    public async Task GetRecentAsync_PassesRequestedDays()
    {
        _repository.GetRecentAsync(5, Arg.Any<CancellationToken>())
            .Returns([]);

        await _service.GetRecentAsync(5);

        await _repository.Received(1).GetRecentAsync(5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetAsDesktopAsync_FetchesImage_AndPassesItToSetter()
    {
        var wallpaper = new Wallpaper
        {
            Date = new DateOnly(2026, 9, 24),
            Title = "测试壁纸",
            ImageUrl = "https://cn.bing.com/th?id=OHR.Test_UHD.jpg",
            ThumbnailUrl = "https://cn.bing.com/th?id=OHR.Test_UHD.jpg&w=384&h=216",
        };
        _repository.GetImageAsync(wallpaper.ImageUrl, Arg.Any<CancellationToken>())
            .Returns(new byte[] { 1, 2, 3 });

        await _service.SetAsDesktopAsync(wallpaper);

        await _repository.Received(1).GetImageAsync(
            wallpaper.ImageUrl, Arg.Any<CancellationToken>());
        await _setter.Received(1).SetAsync(
            Arg.Is<byte[]>(bytes => bytes.SequenceEqual(new byte[] { 1, 2, 3 })),
            "2026-09-24.jpg",
            Arg.Any<CancellationToken>());
    }
}
