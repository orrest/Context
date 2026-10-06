namespace Music.Player;

/// <summary>播放引擎工厂：每曲创建一个独立引擎实例。</summary>
public interface IPlaybackEngineFactory
{
    /// <summary>创建一个尚未打开任何文件的引擎实例。</summary>
    IPlaybackEngine Create();
}

/// <summary>默认引擎工厂：创建 Wasapi 共享模式引擎。</summary>
public sealed class WasapiPlaybackEngineFactory : IPlaybackEngineFactory
{
    /// <inheritdoc />
    public IPlaybackEngine Create()
    {
        return new WasapiPlaybackEngine();
    }
}
