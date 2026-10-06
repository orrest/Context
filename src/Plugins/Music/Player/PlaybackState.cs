namespace Music.Player;

/// <summary>播放器状态：空闲 / 取曲中（扫描来源并打开文件）/ 播放中 / 已暂停。</summary>
public enum PlaybackState
{
    /// <summary>空闲：无播放会话。</summary>
    Idle,

    /// <summary>取曲中：正在向曲目来源取列表并打开文件，尚未出声。</summary>
    Loading,

    /// <summary>播放中：引擎正在输出（曲目衔接的短暂间隙仍视为播放中）。</summary>
    Playing,

    /// <summary>已暂停：会话保留，输出原位暂停，可恢复。</summary>
    Paused,
}
