using Context.Domain.Entities;
using Context.Presentation.ViewModels;
using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using NSubstitute;

namespace Context.Tests;

public class WallpaperViewModelTests
{
    private const string TodayImageUrl = "today";
    private const string YesterdayImageUrl = "yesterday";

    private readonly IWallpaperRepository _repository = Substitute.For<IWallpaperRepository>();
    private readonly IDesktopWallpaperSetter _setter = Substitute.For<IDesktopWallpaperSetter>();

    private WallpaperViewModel CreateViewModel()
    {
        return new WallpaperViewModel(new WallpaperService(_repository, _setter));
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

    [Fact]
    public async Task LoadCommand_FillsWallpapers()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpapers());
        var viewModel = CreateViewModel();

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Wallpapers.Count);
    }

    [Fact]
    public async Task SetNextAsDesktopCommand_FirstClick_LazyLoads_AndSetsToday()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpapers());
        var viewModel = CreateViewModel();

        await viewModel.SetNextAsDesktopCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Wallpapers.Count);
        await _repository.Received(1).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _setter.Received(1).SetAsync(
            Arg.Any<byte[]>(), "2026-09-24.jpg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetNextAsDesktopCommand_WrapsAround_AfterLastWallpaper()
    {
        _repository.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(CreateWallpapers());
        var viewModel = CreateViewModel();

        // 依次：今天 → 昨天 → 回绕到今天
        await viewModel.SetNextAsDesktopCommand.ExecuteAsync(null);
        await viewModel.SetNextAsDesktopCommand.ExecuteAsync(null);
        await viewModel.SetNextAsDesktopCommand.ExecuteAsync(null);

        await _repository.Received(2).GetImageAsync(TodayImageUrl, Arg.Any<CancellationToken>());
        await _repository.Received(1).GetImageAsync(YesterdayImageUrl, Arg.Any<CancellationToken>());
        // 列表只懒加载一次
        await _repository.Received(1).GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
