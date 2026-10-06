using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Music.Player;

/// <summary>
/// NAudio 播放引擎：WasApiOut 共享模式输出 + MediaFoundationReader 解码
/// （不依赖 WMP 旧版组件）。一个实例播一个文件；Dispose 先退订再释放，
/// 释放前在途的 Stopped 回调由持有方以引用比对过滤。
/// </summary>
public sealed class WasapiPlaybackEngine : IPlaybackEngine
{
    private const int LatencyMilliseconds = 200;

    private IWavePlayer? _outputDevice;
    private MediaFoundationReader? _reader;

    /// <inheritdoc />
    public event EventHandler<EngineStoppedEventArgs>? Stopped;

    /// <inheritdoc />
    public void Open(string filePath)
    {
        var reader = new MediaFoundationReader(filePath);
        var output = new WasapiOut(AudioClientShareMode.Shared, LatencyMilliseconds);
        output.PlaybackStopped += OnPlaybackStopped;
        output.Init(reader);
        output.Play();
        _reader = reader;
        _outputDevice = output;
    }

    /// <inheritdoc />
    public void Pause()
    {
        _outputDevice?.Pause();
    }

    /// <inheritdoc />
    public void Resume()
    {
        _outputDevice?.Play();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        var output = _outputDevice;
        if (output is not null)
        {
            _outputDevice = null;
            output.PlaybackStopped -= OnPlaybackStopped;
            try
            {
                output.Stop();
            }
            catch
            {
                // 已停止/未完成初始化
            }

            output.Dispose();
        }

        _reader?.Dispose();
        _reader = null;
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs eventArgs)
    {
        Stopped?.Invoke(this, new EngineStoppedEventArgs(eventArgs.Exception));
    }
}
