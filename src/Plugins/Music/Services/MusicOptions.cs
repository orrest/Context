namespace Music.Services;

/// <summary>音乐插件可变选项：音乐文件目录（随机播放来源），经插件设置项运行期改写。</summary>
public sealed class MusicOptions
{
    /// <summary>音乐文件目录；默认当前用户「音乐」库。</summary>
    public string LibraryDirectory { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
}
