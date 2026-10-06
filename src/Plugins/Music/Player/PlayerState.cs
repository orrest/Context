namespace Music.Player;

/// <summary>
/// 播放器状态基类（State 模式）：把"该状态下各事件如何处置"集中到状态类，
/// 非法事件（如空闲态收暂停、取曲中收切曲）默认忽略、不产生转移。
/// 状态类自身无状态（可变数据全在 <see cref="AudioPlayer"/>），实例按转移即时创建。
/// </summary>
internal abstract class PlayerState
{
    /// <summary>对应的状态枚举值。</summary>
    public abstract PlaybackState Id { get; }

    /// <summary>开播/切下一曲。</summary>
    public virtual Task PlayNext(AudioPlayer player, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>切上一曲。</summary>
    public virtual Task PlayPrevious(AudioPlayer player, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>播放/暂停切换。</summary>
    public virtual Task TogglePlayPause(AudioPlayer player)
    {
        return Task.CompletedTask;
    }

    /// <summary>暂停。</summary>
    public virtual void Pause(AudioPlayer player)
    {
    }

    /// <summary>恢复。</summary>
    public virtual void Resume(AudioPlayer player)
    {
    }

    /// <summary>停止：结束会话。</summary>
    public virtual void Stop(AudioPlayer player)
    {
    }

    /// <summary>引擎传输结束（自然播完/出错）。</summary>
    public virtual void OnEngineStopped(AudioPlayer player, Exception? exception)
    {
    }
}

/// <summary>空闲：无播放会话。播放/切曲事件发起取曲，其余忽略。</summary>
internal sealed class IdleState : PlayerState
{
    /// <inheritdoc />
    public override PlaybackState Id
    {
        get { return PlaybackState.Idle; }
    }

    /// <inheritdoc />
    public override Task PlayNext(AudioPlayer player, CancellationToken cancellationToken)
    {
        return player.BeginLoad(forward: true, cancellationToken);
    }

    /// <inheritdoc />
    public override Task PlayPrevious(AudioPlayer player, CancellationToken cancellationToken)
    {
        return player.BeginLoad(forward: false, cancellationToken);
    }

    /// <inheritdoc />
    public override Task TogglePlayPause(AudioPlayer player)
    {
        // 空闲态无令牌可用：同步入口发起开播
        return player.BeginLoad(forward: true, CancellationToken.None);
    }
}

/// <summary>取曲中：扫描来源并打开文件进行中。仅响应停止（以取消令在途扫描作废）。</summary>
internal sealed class LoadingState : PlayerState
{
    /// <inheritdoc />
    public override PlaybackState Id
    {
        get { return PlaybackState.Loading; }
    }

    /// <inheritdoc />
    public override void Stop(AudioPlayer player)
    {
        player.CancelLoad();
    }
}

/// <summary>播放中：响应全部事件；曲目结束自动连播，连续失败超限熔断回空闲。</summary>
internal class PlayingState : PlayerState
{
    /// <inheritdoc />
    public override PlaybackState Id
    {
        get { return PlaybackState.Playing; }
    }

    /// <inheritdoc />
    public override void Pause(AudioPlayer player)
    {
        player.CurrentEngine?.Pause();
        player.SwitchTo(new PausedState());
    }

    /// <inheritdoc />
    public override void Stop(AudioPlayer player)
    {
        player.StopByUser();
    }

    /// <inheritdoc />
    public override Task PlayNext(AudioPlayer player, CancellationToken cancellationToken)
    {
        return player.BeginLoad(forward: true, cancellationToken);
    }

    /// <inheritdoc />
    public override Task PlayPrevious(AudioPlayer player, CancellationToken cancellationToken)
    {
        return player.BeginLoad(forward: false, cancellationToken);
    }

    /// <inheritdoc />
    public override Task TogglePlayPause(AudioPlayer player)
    {
        Pause(player);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void OnEngineStopped(AudioPlayer player, Exception? exception)
    {
        if (exception is not null)
        {
            // 播放中途失败：跳下一曲；连续失败超限则停止
            player.NoteFailure(exception.Message);
            if (player.ConsecutiveFailures >= AudioPlayer.MaxConsecutiveFailures)
            {
                player.StopWith("连续多个文件播放失败，已停止");
                return;
            }
        }

        // 自然播完：自动连播（音频回调线程发起，无人等待）
        _ = player.BeginLoad(forward: true, CancellationToken.None);
    }
}

/// <summary>已暂停：会话保留，恢复即回播放中；播放/切曲语义与播放中一致。</summary>
internal sealed class PausedState : PlayingState
{
    /// <inheritdoc />
    public override PlaybackState Id
    {
        get { return PlaybackState.Paused; }
    }

    /// <inheritdoc />
    public override void Pause(AudioPlayer player)
    {
        // 已暂停，忽略
    }

    /// <inheritdoc />
    public override void Resume(AudioPlayer player)
    {
        player.CurrentEngine?.Resume();
        player.SwitchTo(new PlayingState());
    }

    /// <inheritdoc />
    public override Task TogglePlayPause(AudioPlayer player)
    {
        Resume(player);
        return Task.CompletedTask;
    }
}
