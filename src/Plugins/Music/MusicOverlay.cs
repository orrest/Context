using System.Windows;
using Music.Views;
using Plugin;

namespace Music;

/// <summary>音乐插件的主窗口覆盖层：承载「喜欢」按钮视图（仅播放音乐时可见）。</summary>
public sealed class MusicOverlay(MusicOverlayView view) : IMainWindowOverlay
{
    public FrameworkElement View
    {
        get { return view; }
    }
}
