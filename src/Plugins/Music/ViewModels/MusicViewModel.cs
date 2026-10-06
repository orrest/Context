using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Music.Player;

namespace Music.ViewModels;

/// <summary>
/// 主窗口覆盖层（播放/暂停按钮）ViewModel：随播放会话显隐（暂停中保持可见以便恢复），
/// 图标与提示随播放/暂停状态切换；命令转调播放器，状态经播放器事件同步。
/// </summary>
public partial class MusicViewModel(AudioPlayer audioPlayer) : ObservableObject
{
    private bool _initialized;

    /// <summary>是否有播放会话（按钮保持可见；暂停中也不隐藏，否则无法用鼠标恢复）。</summary>
    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    /// <summary>是否暂停中（按钮图标随之切换为播放）。</summary>
    [ObservableProperty]
    public partial bool IsPaused { get; private set; }

    /// <summary>当前曲目展示名（文件名去扩展名）。</summary>
    [ObservableProperty]
    public partial string CurrentTitle { get; private set; } = string.Empty;

    /// <summary>按钮 ToolTip（含播放/暂停状态与歌名）。</summary>
    [ObservableProperty]
    public partial string ToolTipText { get; private set; } = string.Empty;

    /// <summary>播放/暂停切换命令（暂停中恢复，播放中暂停，未播放则开播）。</summary>
    public IRelayCommand TogglePlayPauseCommand { get; } = new RelayCommand(audioPlayer.TogglePlayPause);

    /// <summary>视图 Loaded 时调用：订阅播放器状态（视图生命周期与订阅同起止）。</summary>
    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        audioPlayer.StateChanged += (_, _) => Refresh();
        Refresh();
    }

    private void Refresh()
    {
        IsPlaying = audioPlayer.IsPlaying;
        IsPaused = audioPlayer.IsPaused;
        CurrentTitle = audioPlayer.CurrentTrack?.Title ?? string.Empty;
        ToolTipText = audioPlayer.IsPaused
            ? $"已暂停 {CurrentTitle}"
            : $"正在播放 {CurrentTitle}";
    }
}
