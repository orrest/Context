using Context.Domain.Entities;
using Context.UseCases.Abstractions;

namespace Context.UseCases.Services;

public sealed class WallpaperService(IWallpaperRepository repository, IDesktopWallpaperSetter wallpaperSetter)
{
    public async Task<IReadOnlyList<Wallpaper>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
    {
        return await repository.GetRecentAsync(days, cancellationToken);
    }

    public async Task<byte[]> GetImageAsync(string url, CancellationToken cancellationToken = default)
    {
        return await repository.GetImageAsync(url, cancellationToken);
    }

    public async Task SetAsDesktopAsync(Wallpaper wallpaper, CancellationToken cancellationToken = default)
    {
        var imageBytes = await repository.GetImageAsync(wallpaper.ImageUrl, cancellationToken);
        await wallpaperSetter.SetAsync(imageBytes, $"{wallpaper.Date:yyyy-MM-dd}.jpg", cancellationToken);
    }
}
