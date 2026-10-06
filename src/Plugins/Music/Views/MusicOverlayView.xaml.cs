using System.Windows;
using System.Windows.Controls;
using Music.ViewModels;

namespace Music.Views;

public partial class MusicOverlayView : UserControl
{
    private readonly MusicViewModel _viewModel;

    public MusicOverlayView(MusicViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    /// <summary>
    /// Loaded 时订阅播放服务状态（按钮显隐随播放变化）。不经
    /// Interaction.Triggers：Loaded 触发器的附加发生在首次 Loaded 引发期间、
    /// 恰好错过该次引发，状态订阅将无从建立——按词典窗口先例由代码后置触发。
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Initialize();
    }
}
