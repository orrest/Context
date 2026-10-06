using System.Windows;

namespace Plugin;

/// <summary>
/// 主窗口覆盖层：实现类经 DI 注册为 <c>IMainWindowOverlay</c> 后，其视图由宿主主窗口
/// 叠加在桌宠内容之上渲染。视图的显隐与交互完全由插件自治（如仅播放音乐时显示的按钮）；
/// 视图透明区域不参与命中测试，不干扰主窗口拖动。
/// </summary>
public interface IMainWindowOverlay
{
    /// <summary>叠加到主窗口的视图。</summary>
    FrameworkElement View { get; }
}
