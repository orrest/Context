using System.IO;
using System.Runtime.InteropServices;
using Context.UseCases.Abstractions;
using Microsoft.Win32;

namespace Context.Infrastructure.Desktop;

/// <summary>
/// 通过 user32!SystemParametersInfo(SPI_SETDESKWALLPAPER) 设置 Windows 桌面壁纸。
/// </summary>
public sealed class WindowsDesktopWallpaperSetter : IDesktopWallpaperSetter
{
    public Task SetAsync(byte[] imageData, string fileName, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Context", "Wallpapers");
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, fileName);
        File.WriteAllBytes(filePath, imageData);

        // WallpaperStyle: 10 = 填充（按比例铺满），TileWallpaper: 0 = 不平铺
        using (var desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", writable: true))
        {
            desktopKey?.SetValue("WallpaperStyle", "10");
            desktopKey?.SetValue("TileWallpaper", "0");
        }

        if (!SystemParametersInfo(SpiSetDesktopWallpaper, 0, filePath, SpifUpdateIniFile | SpifSendChange))
        {
            throw new InvalidOperationException($"设置桌面壁纸失败（Win32 错误码 {Marshal.GetLastWin32Error()}）。");
        }

        return Task.CompletedTask;
    }

    private const int SpiSetDesktopWallpaper = 0x0014;
    private const int SpifUpdateIniFile = 0x0001;
    private const int SpifSendChange = 0x0002;

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);
}
