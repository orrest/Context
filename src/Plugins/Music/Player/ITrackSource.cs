namespace Music.Player;

/// <summary>
/// 曲目来源契约：播放器由此取得可播放曲目列表（实现方决定扫描范围与排序策略）。
/// </summary>
public interface ITrackSource
{
    /// <summary>取得曲目列表；空列表表示当前无曲可播。</summary>
    Task<IReadOnlyList<AudioTrack>> GetTracksAsync(CancellationToken cancellationToken = default);
}
