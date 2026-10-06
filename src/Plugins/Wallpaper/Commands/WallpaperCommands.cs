using CommunityToolkit.Mvvm.Input;
using Plugin;
using Wallpaper.Services;

namespace Wallpaper.Commands;

/// <summary>
/// 壁纸插件对外声明的命令入口：以 <see cref="HotkeyCommandAttribute"/> 声明命令
/// （id / 展示名 / 默认键手势），经 DI 注册为 <see cref="ICommandProvider"/> 后
/// 由宿主命令目录隐式聚合——壁纸库不感知任何快捷键机制。
/// </summary>
public sealed class WallpaperCommands(WallpaperService wallpaperService) : ICommandProvider
{
    public const string NextCommandId = "wallpaper.next";

    [HotkeyCommand(NextCommandId, "壁纸：切换下一张", "Alt+Down")]
    public IRelayCommand NextCommand { get; } = new AsyncRelayCommand(
        cancellationToken => wallpaperService.SetNextAsDesktopAsync(cancellationToken));
}
