namespace Music.Player;

/// <summary>
/// 单曲播放引擎抽象：一个实例负责一个文件从打开到传输结束的生命周期。
/// 播放器组件经此隔离具体音频实现（NAudio），引擎失败与释放语义对状态机透明。
/// </summary>
public interface IPlaybackEngine : IDisposable
{
    /// <summary>传输结束（自然播完或中途出错）通知，出错时携带异常。</summary>
    event EventHandler<EngineStoppedEventArgs>? Stopped;

    /// <summary>打开文件并开始播放；打开失败（格式/设备问题）抛异常。</summary>
    void Open(string filePath);

    /// <summary>原位暂停：不引发 <see cref="Stopped"/>，不视为传输结束。</summary>
    void Pause();

    /// <summary>从暂停位置继续播放。</summary>
    void Resume();
}

/// <summary>传输结束事件参数：<see cref="Exception"/> 为 null 表示自然播完。</summary>
public sealed class EngineStoppedEventArgs(Exception? exception) : EventArgs
{
    /// <summary>传输中途失败的异常；自然播完时为 null。</summary>
    public Exception? Exception { get; } = exception;
}
