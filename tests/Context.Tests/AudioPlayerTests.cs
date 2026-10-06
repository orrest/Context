using Music.Player;

namespace Context.Tests;

/// <summary>
/// AudioPlayer 状态机转移测试：手写引擎/曲目来源替身（无音频设备、无 SynchronizationContext，
/// 事件同步引发），覆盖转移表主要路径——开播、自动连播、暂停恢复、各态停止、
/// 取曲中停止的取消语义、上一曲回绕、空来源与连续失败熔断、状态事件序列。
/// </summary>
public class AudioPlayerTests
{
    [Fact]
    public async Task PlayNextAsync_FromIdle_OpensFirstTrack_AndEntersPlaying()
    {
        var (player, _, factory) = CreatePlayer();

        await player.PlayNextAsync();

        Assert.Equal(PlaybackState.Playing, player.State);
        Assert.True(player.IsPlaying);
        Assert.Equal("a", player.CurrentTrack?.Title);
        Assert.True(factory.Created[0].Opened);
    }

    [Fact]
    public async Task PlayNextAsync_ExhaustsQueue_RescansSource()
    {
        var (player, source, _) = CreatePlayer();

        await player.PlayNextAsync();
        await player.PlayNextAsync();
        await player.PlayNextAsync();

        Assert.Equal(PlaybackState.Playing, player.State);
        Assert.Equal(2, source.CallCount);
        Assert.Equal("a", player.CurrentTrack?.Title);
    }

    [Fact]
    public async Task EngineStopped_AfterNaturalEnd_AutoAdvancesToNextTrack()
    {
        var (player, _, factory) = CreatePlayer();
        await player.PlayNextAsync();

        factory.Created[0].EndPlayback();

        Assert.Equal(PlaybackState.Playing, player.State);
        Assert.Equal("b", player.CurrentTrack?.Title);
    }

    [Fact]
    public async Task PauseAndResume_TransitionBetweenPlayingAndPaused()
    {
        var (player, _, factory) = CreatePlayer();
        await player.PlayNextAsync();

        player.Pause();

        Assert.Equal(PlaybackState.Paused, player.State);
        Assert.True(player.IsPaused);
        Assert.True(player.IsPlaying); // 暂停仍是会话中（按钮保持可见）
        Assert.True(factory.Created[0].Paused);

        player.Resume();

        Assert.Equal(PlaybackState.Playing, player.State);
        Assert.False(player.IsPaused);
        Assert.False(factory.Created[0].Paused);
    }

    [Fact]
    public async Task Stop_FromPlaying_ReleasesEngine_AndIgnoresStaleStopped()
    {
        var (player, _, factory) = CreatePlayer();
        await player.PlayNextAsync();

        player.Stop();

        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.False(player.IsPlaying);
        Assert.Null(player.CurrentTrack);
        Assert.True(factory.Created[0].Disposed);
        // 已释放的旧引擎在途 Stopped 回调不再引发转移
        factory.Created[0].EndPlayback();
        Assert.Equal(PlaybackState.Idle, player.State);
    }

    [Fact]
    public async Task Stop_DuringLoading_CancelsScan_AndNeverOpensEngine()
    {
        var source = new StubTrackSource("a");
        source.OnGetTracksAsync = token => Task.Delay(Timeout.InfiniteTimeSpan, token);
        var (player, _, factory) = CreatePlayer(source);

        var playTask = player.PlayNextAsync();
        Assert.Equal(PlaybackState.Loading, player.State);

        player.Stop();
        await playTask;

        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.Empty(factory.Created);
        Assert.Null(player.CurrentTrack);
    }

    [Fact]
    public async Task PlayPreviousAsync_FromFirstTrack_WrapsToLast()
    {
        var (player, _, _) = CreatePlayer(new StubTrackSource("a", "b", "c"));
        await player.PlayNextAsync();

        await player.PlayPreviousAsync();

        Assert.Equal("c", player.CurrentTrack?.Title);
    }

    [Fact]
    public async Task PlayPreviousAsync_WithoutSession_StartsFromLastTrack()
    {
        var (player, _, _) = CreatePlayer(new StubTrackSource("a", "b", "c"));

        await player.PlayPreviousAsync();

        Assert.Equal("c", player.CurrentTrack?.Title);
    }

    [Fact]
    public async Task PlayNextAsync_WithEmptySource_EntersIdleWithError()
    {
        var (player, _, factory) = CreatePlayer(new StubTrackSource());

        await player.PlayNextAsync();

        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.False(player.IsPlaying);
        Assert.NotNull(player.LastError);
        Assert.Empty(factory.Created);
    }

    [Fact]
    public async Task OpenFailures_ExceedingLimit_StopsWithCircuitBreak()
    {
        var factory = new FakeEngineFactory { FailOnOpen = true };
        var (player, _, _) = CreatePlayer(factory: factory);

        await player.PlayNextAsync();

        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.Equal(10, factory.Created.Count);
        Assert.NotNull(player.LastError);
    }

    [Fact]
    public async Task EngineFailures_ResetCounter_OnEachSuccessfulAdvance()
    {
        // 播放中途失败换曲、成功开播即清零计数（与既有行为一致）：不触发熔断，继续随机电台
        var titles = Enumerable.Range(0, 5).Select(i => i.ToString()).ToArray();
        var (player, _, factory) = CreatePlayer(new StubTrackSource(titles));
        await player.PlayNextAsync();

        for (var i = 0; i < 4; i++)
        {
            factory.Created[i].EndPlayback(new InvalidOperationException("设备失效"));
        }

        Assert.Equal(PlaybackState.Playing, player.State);
        Assert.Equal(5, factory.Created.Count);
        Assert.Equal("4", player.CurrentTrack?.Title);
    }

    [Fact]
    public async Task TogglePlayPause_FromIdle_StartsPlayback()
    {
        var (player, _, _) = CreatePlayer();

        player.TogglePlayPause();

        Assert.Equal(PlaybackState.Playing, player.State);
    }

    [Fact]
    public async Task TogglePlayPause_FromPaused_Resumes()
    {
        var (player, _, _) = CreatePlayer();
        await player.PlayNextAsync();
        player.Pause();

        player.TogglePlayPause();

        Assert.Equal(PlaybackState.Playing, player.State);
    }

    [Fact]
    public void InvalidEvents_InIdle_AreIgnored()
    {
        var (player, _, factory) = CreatePlayer();

        player.Pause();
        player.Resume();

        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.Empty(factory.Created);
    }

    [Fact]
    public async Task StateChanged_RaisesTransitionSequence()
    {
        // xunit 测试线程携带同步上下文，AudioPlayer 会把 StateChanged 回投其上而延迟引发；
        // 在无线程池上下文中驱动播放器，事件同步引发、序列断言确定
        var transitions = await Task.Run(async () =>
        {
            var (player, _, factory) = CreatePlayer();
            var list = new List<PlaybackState>();
            player.StateChanged += (_, _) => list.Add(player.State);

            await player.PlayNextAsync();
            player.Pause();
            player.Resume();
            factory.Created[0].EndPlayback();
            player.Stop();
            return list;
        });

        Assert.Equal(
            [
                PlaybackState.Loading, PlaybackState.Playing,
                PlaybackState.Paused,
                PlaybackState.Playing,
                PlaybackState.Loading, PlaybackState.Playing,
                PlaybackState.Idle,
            ],
            transitions);
    }

    private static (AudioPlayer Player, StubTrackSource Source, FakeEngineFactory Factory) CreatePlayer(
        StubTrackSource? source = null,
        FakeEngineFactory? factory = null)
    {
        source ??= new StubTrackSource("a", "b");
        factory ??= new FakeEngineFactory();
        return (new AudioPlayer(source, factory), source, factory);
    }

    /// <summary>曲目来源替身：返回给定曲目，可编程注入取曲延迟/取消以模拟扫描进行中。</summary>
    private sealed class StubTrackSource(params string[] titles) : ITrackSource
    {
        public int CallCount { get; private set; }

        public Func<CancellationToken, Task>? OnGetTracksAsync { get; set; }

        public async Task<IReadOnlyList<AudioTrack>> GetTracksAsync(CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (OnGetTracksAsync is not null)
            {
                await OnGetTracksAsync(cancellationToken);
            }

            return titles
                .Select(title => new AudioTrack($"/music/{title}.mp3", title))
                .ToArray();
        }
    }

    /// <summary>引擎替身：记录打开/暂停/释放，测试可手动引发传输结束。</summary>
    private sealed class FakeEngine : IPlaybackEngine
    {
        public event EventHandler<EngineStoppedEventArgs>? Stopped;

        public bool Opened { get; private set; }

        public bool Paused { get; private set; }

        public bool Disposed { get; private set; }

        public bool FailOnOpen { get; init; }

        public void Open(string filePath)
        {
            if (FailOnOpen)
            {
                throw new InvalidOperationException("格式不支持");
            }

            Opened = true;
        }

        public void Pause()
        {
            Paused = true;
        }

        public void Resume()
        {
            Paused = false;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        /// <summary>模拟传输结束（自然播完传 null，出错传异常）。</summary>
        public void EndPlayback(Exception? exception = null)
        {
            Stopped?.Invoke(this, new EngineStoppedEventArgs(exception));
        }
    }

    /// <summary>引擎工厂替身：收集创建的引擎，可统一标记打开失败。</summary>
    private sealed class FakeEngineFactory : IPlaybackEngineFactory
    {
        public List<FakeEngine> Created { get; } = [];

        public bool FailOnOpen { get; init; }

        public IPlaybackEngine Create()
        {
            var engine = new FakeEngine { FailOnOpen = FailOnOpen };
            Created.Add(engine);
            return engine;
        }
    }
}
