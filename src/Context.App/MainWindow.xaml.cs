using System.Windows;
using Context.Presentation.ViewModels;

namespace Context.App;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
