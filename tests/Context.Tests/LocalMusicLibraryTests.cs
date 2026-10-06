using Music.Services;

namespace Context.Tests;

public class LocalMusicLibraryTests
{
    [Fact]
    public async Task GetTracksAsync_ScansRecursively_AndFiltersSupportedExtensions()
    {
        var root = CreateTempDirectory();
        try
        {
            File.WriteAllText(Path.Combine(root, "第一首.mp3"), string.Empty);
            File.WriteAllText(Path.Combine(root, "第二首.FLAC"), string.Empty); // 扩展名大小写不敏感
            File.WriteAllText(Path.Combine(root, "忽略.txt"), string.Empty);
            var sub = Directory.CreateDirectory(Path.Combine(root, "子目录"));
            File.WriteAllText(Path.Combine(sub.FullName, "第三首.m4a"), string.Empty);

            var library = new LocalMusicLibrary(new MusicOptions { LibraryDirectory = root });

            var tracks = await library.GetTracksAsync();

            Assert.Equal(3, tracks.Count);
            Assert.All(tracks, track =>
                Assert.Equal(Path.GetFileNameWithoutExtension(track.FilePath), track.Title));
            Assert.Contains(tracks, track => track.Title == "第一首");
            Assert.Contains(tracks, track => track.Title == "第二首");
            Assert.Contains(tracks, track => track.Title == "第三首");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task GetTracksAsync_ReturnsEmpty_WhenDirectoryMissing()
    {
        var library = new LocalMusicLibrary(new MusicOptions
        {
            LibraryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
        });

        Assert.Empty(await library.GetTracksAsync());
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
