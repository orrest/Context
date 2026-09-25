using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Context.App;

public partial class PetView : UserControl
{
    public PetView()
    {
        InitializeComponent();
    }

    private void OnPetMouseEnter(object sender, MouseEventArgs e)
    {
        ActionButtonPanel.Visibility = Visibility.Visible;
    }

    private void OnPetMouseLeave(object sender, MouseEventArgs e)
    {
        ActionButtonPanel.Visibility = Visibility.Collapsed;
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
