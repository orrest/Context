using System.Windows;
using System.Windows.Controls;

namespace Context.App;

public partial class PetView : UserControl
{
    public PetView()
    {
        InitializeComponent();
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        // MenuItem 位于 Popup 可视树，须经宿主控件反查所在窗口
        Window.GetWindow(PetRoot)?.Close();
    }
}
