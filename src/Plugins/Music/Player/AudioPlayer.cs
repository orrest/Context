namespace Music.Player;

/// <summary>
/// 音频播放器（单例门面）：以状态机（Idle/Loading/Playing/Paused）管理播放会话——
/// 从注入的曲目来源随机衔接取曲，经可替换的播放引擎出声；列表耗尽自动重扫
/// （顺带拾取新增文件），曲目结束自动下一曲，连续失败超限熔断回空闲。
/// 暂停/恢复走引擎原位暂停；停止即结束会话，取曲进行中停止则以取消令在途扫描作废。
/// 状态变更经构造时捕获的 UI SynchronizationContext 回投通知；切曲/停止先退订引擎
/// 事件再释放，避免陈旧 Stopped 误触发自动连播。
/// </summary>
public sealed class AudioPlayer : IDisposable
{
    /// <summary>连续打开/播放失败达到该次数即熔断停止。</summary>
    internal const int MaxConsecutiveFailures = 10;

    private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;
    private readonly ITrackSource _trackSource;
    private readonly IPlaybackEngineFactory _engineFactory;

    private PlayerState _state;
    private IPlaybackEngine? _engine;
    private IReadOnlyList<AudioTrack> _queue = [];
    private int _position = -1;
    private int _consecutiveFailures;
    private CancellationTokenSource? _loadCts;

    /// <summary>构造播放器：绑定曲目来源与引擎工厂（均为纯引用赋值，可在线程亲和约束外安全构造）。</summary>
    public AudioPlayer(ITrackSource trackSource, IPlaybackEngineFactory engineFactory)
    {
        _trackSource = trackSource;
        _engineFactory = engineFactory;
        _state = new IdleState();
    }

    /// <summary>状态变更（状态转移）通知，供视图刷新。</summary>
    public event EventHandler? StateChanged;

    /// <summary>当前状态机状态。</summary>
    public PlaybackState State
    {
        get { return _state.Id; }
    }

    /// <summary>是否有播放会话（取曲/衔接间隙/暂停期间均算，避免按钮闪烁/消失）。</summary>
    public bool IsPlaying
    {
        get { return _state.Id is not PlaybackState.Idle; }
    }

    /// <summary>是否暂停中。</summary>
    public bool IsPaused
    {
        get { return _state.Id is PlaybackState.Paused; }
    }

    /// <summary>当前曲目；无会话时为 null。</summary>
    public AudioTrack? CurrentTrack { get; private set; }

    /// <summary>最近一次失败原因（null 表示正常）。</summary>
    public string? LastError { get; internal set; }

    /// <summary>当前引擎（状态类切换/暂停/恢复用）。</summary>
    internal IPlaybackEngine? CurrentEngine
    {
        get { return _engine; }
    }

    /// <summary>连续失败计数（状态类熔断判断用）。</summary>
    internal int ConsecutiveFailures
    {
        get { return _consecutiveFailures; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CancelLoad();
        Unload();
    }

    /// <summary>播放下一曲：列表耗尽即重扫来源；来源为空或连续失败超限则停止。</summary>
    public Task PlayNextAsync(CancellationToken cancellationToken = default)
    {
        return _state.PlayNext(this, cancellationToken);
    }

    /// <summary>播放上一曲：队列内回绕到队尾；本会话从未取曲则先取列表并从队尾开始。</summary>
    public Task PlayPreviousAsync(CancellationToken cancellationToken = default)
    {
        return _state.PlayPrevious(this, cancellationToken);
    }

    /// <summary>播放/暂停切换：暂停中恢复，播放中暂停，空闲则开播。</summary>
    public void TogglePlayPause()
    {
        _ = _state.TogglePlayPause(this);
    }

    /// <summary>暂停：输出原位暂停（不引发 Stopped，不触发自动连播），会话保留。</summary>
    public void Pause()
    {
        _state.Pause(this);
    }

    /// <summary>恢复：从暂停位置继续播放。</summary>
    public void Resume()
    {
        _state.Resume(this);
    }

    /// <summary>停止：结束会话（覆盖层随之隐藏），再次播放/切曲即重新开始。</summary>
    public void Stop()
    {
        _state.Stop(this);
    }

    /// <summary>发起取曲（各运行态的播放/切曲事件汇聚于此）：先切 Loading 挡重入，再驱动取曲循环。</summary>
    internal Task BeginLoad(bool forward, CancellationToken externalToken)
    {
        SwitchTo(new LoadingState());
        return LoadAsync(forward, externalToken);
    }

    /// <summary>取消在途取曲（取曲中收到停止）：取曲循环以取消语义收场回空闲。</summary>
    internal void CancelLoad()
    {
        _loadCts?.Cancel();
    }

    /// <summary>用户停止：清空错误并回空闲。</summary>
    internal void StopByUser()
    {
        LastError = null;
        Unload();
    }

    /// <summary>因失败/无曲停止：记录原因并回空闲。</summary>
    internal void StopWith(string reason)
    {
        LastError = reason;
        Unload();
    }

    /// <summary>记录一次播放失败（失败原因 + 连续失败计数）。</summary>
    internal void NoteFailure(string message)
    {
        LastError = message;
        _consecutiveFailures++;
    }

    /// <summary>状态转移（状态类专用）。</summary>
    internal void SwitchTo(PlayerState state)
    {
        _state = state;
        NotifyStateChanged();
    }

    /// <summary>取曲循环：向来源取列表、按方向推进队列位置并逐曲尝试打开，成功即换装引擎进入播放。</summary>
    private async Task LoadAsync(bool forward, CancellationToken externalToken)
    {
        var loadCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _loadCts = loadCts;
        try
        {
            while (true)
            {
                if (forward)
                {
                    if (_position + 1 >= _queue.Count)
                    {
                        _queue = await _trackSource.GetTracksAsync(loadCts.Token);
                        _position = -1;
                        if (_queue.Count == 0)
                        {
                            StopWith("音乐目录中没有可播放的文件");
                            return;
                        }
                    }

                    _position++;
                }
                else
                {
                    if (_queue.Count == 0)
                    {
                        _queue = await _trackSource.GetTracksAsync(loadCts.Token);
                        if (_queue.Count == 0)
                        {
                            StopWith("音乐目录中没有可播放的文件");
                            return;
                        }

                        // 队首前置一位：首次取曲即回绕到队尾
                        _position = _queue.Count;
                    }

                    _position--;
                    if (_position < 0)
                    {
                        _position = _queue.Count - 1;
                    }
                }

                CurrentTrack = _queue[_position];
                var engine = _engineFactory.Create();
                engine.Stopped += OnEngineStopped;
                try
                {
                    engine.Open(CurrentTrack.FilePath);
                }
                catch (Exception exception)
                {
                    // 打开失败（格式/设备问题）：跳下一/上一曲；连续失败超限则停止
                    engine.Stopped -= OnEngineStopped;
                    engine.Dispose();
                    NoteFailure(exception.Message);
                    if (_consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        StopWith("连续多个文件播放失败，已停止");
                        return;
                    }

                    continue;
                }

                // 换装新引擎（旧引擎此刻才释放：取曲期间旧曲继续出声，与既有行为一致），
                // 开播前复核取消——停止请求在出声前总能胜出，被取消时由 Unload 释放新引擎
                DisposeEngine();
                _engine = engine;
                _consecutiveFailures = 0;
                loadCts.Token.ThrowIfCancellationRequested();
                SwitchTo(new PlayingState());
                return;
            }
        }
        catch (OperationCanceledException)
        {
            // 取曲被取消（用户停止/退出/命令取消）：不开播，回空闲
            StopByUser();
        }
        catch (Exception exception)
        {
            StopWith(exception.Message);
        }
        finally
        {
            if (ReferenceEquals(_loadCts, loadCts))
            {
                _loadCts = null;
            }

            loadCts.Dispose();
        }
    }

    private void OnEngineStopped(object? sender, EngineStoppedEventArgs eventArgs)
    {
        // 仅当前引擎且处于播放态的结束回调参与自动连播；切曲/停止已释放的旧引擎在途回调忽略
        if (!ReferenceEquals(sender, _engine) || _state.Id is not PlaybackState.Playing)
        {
            return;
        }

        _state.OnEngineStopped(this, eventArgs.Exception);
    }

    /// <summary>结束会话：释放引擎、清空曲目、回空闲。</summary>
    private void Unload()
    {
        DisposeEngine();
        CurrentTrack = null;
        SwitchTo(new IdleState());
    }

    private void DisposeEngine()
    {
        var engine = _engine;
        if (engine is not null)
        {
            _engine = null;
            engine.Stopped -= OnEngineStopped;
            engine.Dispose();
        }
    }

    private void NotifyStateChanged()
    {
        if (_synchronizationContext is { } context)
        {
            context.Post(_ => StateChanged?.Invoke(this, EventArgs.Empty), null);
        }
        else
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
