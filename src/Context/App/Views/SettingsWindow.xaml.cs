using System.Windows;
using Context.Presentation.ViewModels;
using Orrest.Ui.Controls;

namespace Context.App.Views;

public partial class SettingsWindow : OWindow
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
