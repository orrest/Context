using System.Windows;
using Context.Presentation.ViewModels;

namespace Context.App;

public partial class DictionaryWindow : Window
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
