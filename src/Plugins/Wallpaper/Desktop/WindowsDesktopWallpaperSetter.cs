using System.IO;
using System.Runtime.InteropServices;
using Wallpaper.Abstractions;
using Microsoft.Win32;

namespace Wallpaper.Desktop;

/// <summary>
/// 通过 user32!SystemParametersInfo(SPI_SETDESKWALLPAPER) 设置 Windows 桌面壁纸。
/// </summary>
public sealed partial class WindowsDesktopWallpaperSetter : IDesktopWallpaperSetter
{
    public Task SetAsync(byte[] imageData, string fileName, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Context", "Wallpaper");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, fileName);
        File.WriteAllBytes(filePath, imageData);

        // WallpaperStyle: 10 = 填充（按比例铺满），TileWallpaper: 0 = 不平铺
        using (var desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true))
        {
            desktopKey?.SetValue("WallpaperStyle", "10");
            desktopKey?.SetValue("TileWallpaper", "0");
        }

        if (!SystemParametersInfoW(SpiSetDesktopWallpaper, 0, filePath, SpifUpdateIniFile | SpifSendChange))
        {
            throw new InvalidOperationException($"设置桌面壁纸失败（Win32 错误码 {Marshal.GetLastWin32Error()}）。");
        }

        return Task.CompletedTask;
    }

    private const int SpiSetDesktopWallpaper = 0x0014;
    private const int SpifUpdateIniFile = 0x0001;
    private const int SpifSendChange = 0x0002;

    // 声明不带 extern：本机 SDK 对 extern partial 配对存在缺陷（CS0757/CS0179）；
    // 方法名与精确导出名一致（SystemParametersInfoW），ExactSpelling 下不做 A/W 自动解析。
    [LibraryImport("user32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SystemParametersInfoW(int uiAction, int uiParam, string pvParam, int fWinIni);
}
