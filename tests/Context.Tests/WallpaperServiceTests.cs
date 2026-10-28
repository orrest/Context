using Wallpaper.Abstractions;
using WallpaperEntity = Wallpaper.Entities.Wallpaper;
using Wallpaper.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperServiceTests
{
    private const string TodayImageUrl = "today";
    private const string YesterdayImageUrl = "yesterday";
    private const string OlderImageUrl = "older";

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
            .Returns(CreateWallpaper());

        await _service.SetNextAsDesktopAsync();

        await _repository.Received(1).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), $"{Today():yyyy-MM-dd}.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_WrapsAround_AfterLastWallpaper()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpaper());

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
            .Returns([], CreateWallpaper());

        await _service.SetNextAsDesktopAsync();
        await _setter.DidNotReceiveWithAnyArgs().SetAsync(
            Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        await _service.SetNextAsDesktopAsync();

        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), $"{Today():yyyy-MM-dd}.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_CacheNewestIsNotToday_RefetchesEveryCall()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateStaleWallpaper());

        await _service.SetNextAsDesktopAsync();
        await _service.SetNextAsDesktopAsync();

        await _repository.Received(2).GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_RefetchBringsNewDay_RestartsFromToday()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateStaleWallpaper(), CreateWallpaper());

        await _service.SetNextAsDesktopAsync();
        await _service.SetNextAsDesktopAsync();

        await _repository.Received(1).GetImageAsync(YesterdayImageUrl, Arg.Any<CancellationToken>());
        await _repository.Received(1).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), $"{Today():yyyy-MM-dd}.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopAsync_RefetchSameNewestDay_KeepsBrowsePosition()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateStaleWallpaper(), CreateStaleWallpaper());

        await _service.SetNextAsDesktopAsync();
        await _service.SetNextAsDesktopAsync();

        await _repository.Received(1).GetImageAsync(YesterdayImageUrl, Arg.Any<CancellationToken>());
        await _repository.Received(1).GetImageAsync(OlderImageUrl, Arg.Any<CancellationToken>());
    }

    private static DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.Now);
    }

    private static List<WallpaperEntity> CreateWallpaper()
    {
        return
        [
            new WallpaperEntity
            {
                Date = Today(),
                Title = "今天",
                ImageUrl = TodayImageUrl,
                ThumbnailUrl = "today-thumb",
            },
            new WallpaperEntity
            {
                Date = Today().AddDays(-1),
                Title = "昨天",
                ImageUrl = YesterdayImageUrl,
                ThumbnailUrl = "yesterday-thumb",
            },
        ];
    }

    // Newest wallpaper is dated yesterday, as when the app outlives midnight before the archive updates.
    // 最新壁纸日期为昨天，模拟应用跨天驻留且归档尚未更新的场景。
    private static List<WallpaperEntity> CreateStaleWallpaper()
    {
        return
        [
            new WallpaperEntity
            {
                Date = Today().AddDays(-1),
                Title = "昨天",
                ImageUrl = YesterdayImageUrl,
                ThumbnailUrl = "yesterday-thumb",
            },
            new WallpaperEntity
            {
                Date = Today().AddDays(-2),
                Title = "前天",
                ImageUrl = OlderImageUrl,
                ThumbnailUrl = "older-thumb",
            },
        ];
    }
}
