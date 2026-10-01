using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperServiceTests
{
    private const string TodayImageUrl = "today";
    private const string YesterdayImageUrl = "yesterday";

    private readonly IWallpaperRepository _repository = Substitute.For<IWallpaperRepository>();
    private readonly IDesktopWallpaperSetter _setter = Substitute.For<IDesktopWallpaperSetter>();
    private readonly WallpaperService _service;

    public WallpaperServiceTests()
    {
        _service = new WallpaperService(_repository, _setter);
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_FirstCall_LazyLoads_AndSetsToday()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpapers());

        await _service.SetNextAsDesktopAsync();

        await _repository.Received(1).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), "2026-09-24.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_WrapsAround_AfterLastWallpaper()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpapers());

        // 依次：今天 → 昨天 → 回绕到今天
        await _service.SetNextAsDesktopAsync();
        await _service.SetNextAsDesktopAsync();
        await _service.SetNextAsDesktopAsync();

        await _repository.Received(2).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _repository.Received(1).GetImageAsync(YesterdayImageUrl, Arg.Any<CancellationToken>());
        // 列表只懒加载一次
        await _repository.Received(1).GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_EmptyRecentList_DoesNotSet_AndRetriesNextCall()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([], CreateWallpapers());

        await _service.SetNextAsDesktopAsync();
        await _setter.DidNotReceiveWithAnyArgs().SetAsync(
            Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _service.SetNextAsDesktopAsync();

        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), "2026-09-24.jpg", Arg.Any<CancellationToken>());
    }

    private static List<Wallpaper> CreateWallpapers()
    {
        return
        [
            new Wallpaper
            {
                Date = new DateOnly(2026, 9, 24),
                Title = "今天",
                ImageUrl = TodayImageUrl,
                ThumbnailUrl = "today-thumb",
            },
            new Wallpaper
            {
                Date = new DateOnly(2026, 9, 23),
                Title = "昨天",
                ImageUrl = YesterdayImageUrl,
                ThumbnailUrl = "yesterday-thumb",
            },
        ];
    }
}
