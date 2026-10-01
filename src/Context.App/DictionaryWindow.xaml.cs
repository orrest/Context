using System.Windows;
using Context.Presentation.ViewModels;
using Orrest.Ui.Controls;

namespace Context.App;

public partial class DictionaryWindow : OWindow
{
    public DictionaryWindow(DictionaryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }
}
