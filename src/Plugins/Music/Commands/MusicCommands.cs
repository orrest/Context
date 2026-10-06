using CommunityToolkit.Mvvm.Input;
using Music.Player;
using Plugin;

namespace Music.Commands;

/// <summary>
/// 音乐插件对外声明的命令入口：以 <see cref="HotkeyCommandAttribute"/> 声明命令
/// （id / 展示名 / 默认键手势），经 DI 注册为 <see cref="ICommandProvider"/> 后
/// 由宿主插件目录隐式聚合——播放器组件不感知任何快捷键机制。
/// </summary>
public sealed class MusicCommands(AudioPlayer audioPlayer) : ICommandProvider
{
    public const string NextCommandId = "music.next";
    public const string PreviousCommandId = "music.prev";
    public const string PlayPauseCommandId = "music.playPause";
    public const string StopCommandId = "music.stop";

    // 默认手势用 OEM 键枚举名：WPF KeyGestureConverter 不解析单标点串（"Alt+." 会装配失败），
    // 枚举名与设置页改绑（GetDisplayStringForCulture）产出的串同构、可回程解析
    [HotkeyCommand(NextCommandId, "音乐：下一曲", "Alt+OemPeriod")]
    public IRelayCommand NextCommand { get; } = new AsyncRelayCommand(
        cancellationToken => audioPlayer.PlayNextAsync(cancellationToken));

    [HotkeyCommand(PreviousCommandId, "音乐：上一曲", "Alt+OemComma")]
    public IRelayCommand PreviousCommand { get; } = new AsyncRelayCommand(
        cancellationToken => audioPlayer.PlayPreviousAsync(cancellationToken));

    [HotkeyCommand(PlayPauseCommandId, "音乐：播放/暂停", "Alt+Space")]
    public IRelayCommand PlayPauseCommand { get; } = new RelayCommand(audioPlayer.TogglePlayPause);

    [HotkeyCommand(StopCommandId, "音乐：停止播放", "Alt+Oem2")]
    public IRelayCommand StopCommand { get; } = new RelayCommand(audioPlayer.Stop);
}
