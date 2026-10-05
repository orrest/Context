namespace Wallpaper.Abstractions;

public interface IDesktopWallpaperSetter
{
    /// <summary>将图片内容写入本地并设为 Windows 桌面壁纸。</summary>
    /// <param name="imageData">图片二进制内容。</param>
    /// <param name="fileName">落盘文件名（如 2026-09-24.jpg）。</param>
    Task SetAsync(byte[] imageData, string fileName, CancellationToken cancellationToken = default);
}
