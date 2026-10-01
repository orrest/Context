using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
using Orrest.Ui.Themes;

namespace Context.App;

public partial class MainWindow : Window, IRecipient<DictionaryRequestedMessage>, IRecipient<ThemeChangeRequestedMessage>
{
    private readonly Func<DictionaryWindow> _dictionaryWindowFactory;
    private readonly IMessenger _messenger;
    private DictionaryWindow? _dictionaryWindow;

    public MainWindow(MainViewModel viewModel, Func<DictionaryWindow> dictionaryWindowFactory, IMessenger messenger)
    {
        InitializeComponent();
        DataContext = viewModel;
        _dictionaryWindowFactory = dictionaryWindowFactory;
        _messenger = messenger;
        _messenger.RegisterAll(this);
        Closed += (_, _) => _messenger.UnregisterAll(this);
    }

    public void Receive(DictionaryRequestedMessage message)
    {
        if (_dictionaryWindow is { IsVisible: true })
        {
            _dictionaryWindow.Activate();
            return;
        }

        _dictionaryWindow = _dictionaryWindowFactory();
        _dictionaryWindow.Show();
    }

    public void Receive(ThemeChangeRequestedMessage message)
    {
        ThemeManager.Apply(message.IsDark ? AppTheme.Dark : AppTheme.Light);
    }

    private void OnWindowDrag(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
