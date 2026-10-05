using System.Windows;
using Dictionary.ViewModels;
using Orrest.Ui.Controls;

namespace Dictionary.Views;

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
