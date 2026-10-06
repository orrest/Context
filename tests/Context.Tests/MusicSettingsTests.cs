using Music.Services;
using Music.Settings;

namespace Context.Tests;

public class MusicSettingsTests
{
    [Fact]
    public void LibraryDirectory_SettingIdIsLibraryDirectory()
    {
        Assert.Equal("music.libraryDirectory", MusicSettings.LibraryDirectorySettingId);
    }

    [Fact]
    public void LibraryDirectory_DefaultsToUserMusicFolder()
    {
        var settings = new MusicSettings(new MusicOptions());

        Assert.Equal(
            Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
            settings.LibraryDirectory);
    }

    [Fact]
    public void LibraryDirectory_DelegatesToOptions()
    {
        var options = new MusicOptions();
        var settings = new MusicSettings(options);

        settings.LibraryDirectory = @"D:\Music";

        Assert.Equal(@"D:\Music", options.LibraryDirectory);
        Assert.Equal(options.LibraryDirectory, settings.LibraryDirectory);
    }
}
