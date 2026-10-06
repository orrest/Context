using Music.Services;
using Plugin;

namespace Music.Settings;

/// <summary>
/// 音乐插件对外声明的设置项入口：以 <see cref="PluginSettingAttribute"/> 声明插件特有设置
/// （id / 展示名），经 DI 注册为 <see cref="ICommandProvider"/> 后由宿主插件目录隐式聚合；
/// 属性读写委托 <see cref="MusicOptions"/> 单例，设置页修改经声明回写即时生效。
/// </summary>
public sealed class MusicSettings(MusicOptions musicOptions) : ICommandProvider
{
    public const string LibraryDirectorySettingId = "music.libraryDirectory";

    [PluginSetting(LibraryDirectorySettingId, "音乐：音乐目录")]
    public string LibraryDirectory
    {
        get { return musicOptions.LibraryDirectory; }
        set { musicOptions.LibraryDirectory = value; }
    }
}
