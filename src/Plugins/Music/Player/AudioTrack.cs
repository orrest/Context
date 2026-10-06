namespace Music.Player;

/// <summary>可播放曲目（播放器组件的曲目模型）：文件路径与展示名。</summary>
public sealed record AudioTrack(string FilePath, string Title);
