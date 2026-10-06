using Microsoft.Extensions.DependencyInjection;

namespace Music.Player;

/// <summary>播放器组件装配：引擎工厂与播放器单例（组件不感知快捷键、UI 与具体曲目来源）。</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAudioPlayer(this IServiceCollection services)
    {
        services.AddSingleton<IPlaybackEngineFactory, WasapiPlaybackEngineFactory>();
        services.AddSingleton<AudioPlayer>();
        return services;
    }
}
