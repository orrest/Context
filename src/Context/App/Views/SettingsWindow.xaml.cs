using System.Windows;
using Context.Presentation.ViewModels;
using Orrest.Ui.Controls;

namespace Context.App.Views;

public partial class SettingsWindow : OWindow
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    /// <summary>
    /// Loaded 时加载设置与快捷键/插件设置条目。不经 Interaction.Triggers：
    /// 窗口级触发器的附加发生在首次 Loaded 引发期间、恰好错过该次引发（窗口只引发一次），
    /// 命令将永不执行——按词典窗口先例由代码后置触发。
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.InitializeCommand.Execute(null);
    }
}
