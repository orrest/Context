using System.IO;
using Music.Player;

namespace Music.Services;

/// <summary>本地音乐库：递归扫描音乐目录下受支持格式的音频文件，随机排序返回（播放器组件的曲目来源）。</summary>
public sealed class LocalMusicLibrary(MusicOptions options) : ITrackSource
{
    // Windows Media Foundation 解码：mp3/flac/m4a/aac/wav/wma 可播，ogg 不支持
    private static readonly string[] SupportedExtensions = [".mp3", ".flac", ".m4a", ".aac", ".wav", ".wma"];

    /// <inheritdoc />
    public Task<IReadOnlyList<AudioTrack>> GetTracksAsync(CancellationToken cancellationToken = default)
    {
        var directory = options.LibraryDirectory;
        if (!Directory.Exists(directory))
        {
            return Task.FromResult<IReadOnlyList<AudioTrack>>([]);
        }

        var tracks = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(path => SupportedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
            .Select(path => new AudioTrack(path, Path.GetFileNameWithoutExtension(path)))
            .ToArray();

        // 随机打散：每次刷新队列顺序不同
        Random.Shared.Shuffle(tracks);
        return Task.FromResult<IReadOnlyList<AudioTrack>>(tracks);
    }
}
