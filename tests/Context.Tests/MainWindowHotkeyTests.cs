using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Context.App;
using Context.App.Views;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Dictionary;
using Music;
using Music.Player;
using Music.Services;
using Microsoft.Extensions.DependencyInjection;
using Slime;
using Wallpaper;

namespace Context.Tests;

/// <summary>
/// 主窗口热键链路集成测试（STA）：验证插件目录 → InputBindings（KeyBinding）→
/// 命令执行 → 播放状态的真实触发路径，使用可真实解码的合成 WAV 作为曲目。
/// </summary>
public class MainWindowHotkeyTests
{
    [Fact]
    public void MainWindow_AltPeriodKeyBinding_UnderSta_TriggersMusicNext()
    {
        Exception? error = null;
        var done = new ManualResetEventSlim(false);
        var bindingFound = false;
        var isPlaying = false;
        string? currentTitle = null;

        var thread = new Thread(() =>
        {
            try
            {
                var dbPath = Path.Combine(Path.GetTempPath(), $"context-hotkey-{Guid.NewGuid():N}.db");
                var musicDir = Path.Combine(Path.GetTempPath(), $"context-music-{Guid.NewGuid():N}");
                Directory.CreateDirectory(musicDir);
                WriteWav(Path.Combine(musicDir, "track.wav"));

                var services = new ServiceCollection();
                services.AddPresentation()
                    .AddInfrastructure($"Data Source={dbPath}")
                    .AddWindows()
                    .AddHotkeys()
                    .AddDictionary()
                    .AddWallpaper()
                    .AddMusic()
                    .AddSlime();

                using var provider = services.BuildServiceProvider();
                provider.GetRequiredService<DatabaseInitializer>().InitializeAsync().GetAwaiter().GetResult();
                // 播放目录指向测试曲目（服务消费该属性发生在播放时，构造后改写即可）
                provider.GetRequiredService<MusicOptions>().LibraryDirectory = musicDir;

                var window = provider.GetRequiredService<MainWindow>();
                var musicPlayer = provider.GetRequiredService<AudioPlayer>();

                window.Show();

                // 等待构造中异步发起的 InputBindings 装配完成
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < 5000 && window.InputBindings.Count == 0)
                {
                    PumpDispatcher();
                }

                var binding = window.InputBindings
                    .OfType<KeyBinding>()
                    .FirstOrDefault(item => item.Gesture is KeyGesture gesture && IsAltPeriod(gesture));
                bindingFound = binding is not null;

                if (binding is not null)
                {
                    var asyncMusicCommand = (IAsyncRelayCommand)binding.Command;
                    binding.Command.Execute(null);

                    // 等待命令异步完成（选曲 + WasApiOut/Play 调用）
                    while (stopwatch.ElapsedMilliseconds < 5000
                           && asyncMusicCommand.ExecutionTask is { IsCompleted: false })
                    {
                        PumpDispatcher();
                    }

                    // NAudio 不依赖 WMP 旧组件：WasApiOut 初始化成功即进入播放状态
                    isPlaying = musicPlayer.IsPlaying;
                    currentTitle = musicPlayer.CurrentTrack?.Title;
                }

                window.Close();
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                done.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(done.Wait(40000), "STA 线程超时");
        if (error is not null)
        {
            throw error;
        }

        Assert.True(bindingFound, "主窗口 InputBindings 中未找到 Alt+. 绑定（快捷键未装配）");
        Assert.True(isPlaying, $"Alt+. 命令已触发但未进入播放状态；当前曲目：{currentTitle ?? "null"}");
        Assert.Equal("track", currentTitle);
    }

    private static bool IsAltPeriod(KeyGesture gesture)
    {
        return gesture.Modifiers == ModifierKeys.Alt && gesture.Key == Key.OemPeriod;
    }

    private static void PumpDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>生成可被 Media Foundation 真实解码的单声道 8kHz 16bit 正弦 WAV。</summary>
    private static void WriteWav(string path)
    {
        const int sampleRate = 8000;
        const short channels = 1;
        const short bitsPerSample = 16;
        var dataLength = sampleRate * channels * bitsPerSample / 8; // 1 秒

        using var writer = new BinaryWriter(File.Create(path));
        writer.Write([0x52, 0x49, 0x46, 0x46]); // "RIFF"
        writer.Write(36 + dataLength);
        writer.Write([0x57, 0x41, 0x56, 0x45]); // "WAVE"
        writer.Write([0x66, 0x6D, 0x74, 0x20]); // "fmt "
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write([0x64, 0x61, 0x74, 0x61]); // "data"
        writer.Write(dataLength);
        for (var i = 0; i < dataLength / 2; i++)
        {
            writer.Write((short)(Math.Sin(i * 0.1) * 10000));
        }
    }
}
