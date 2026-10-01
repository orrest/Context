using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.Presentation.Messages;
using Context.Presentation.ViewModels;
using Orrest.Ui.Themes;

namespace Context.App;

public partial class MainWindow :
    Window,
    IRecipient<DictionaryRequestedMessage>,
    IRecipient<SettingsRequestedMessage>,
    IRecipient<SettingsAppliedMessage>
{
    private readonly Func<DictionaryWindow> _dictionaryWindowFactory;
    private readonly Func<SettingsWindow> _settingsWindowFactory;
    private readonly IMessenger _messenger;
    private DictionaryWindow? _dictionaryWindow;
    private SettingsWindow? _settingsWindow;

    public MainWindow(
        MainViewModel viewModel,
        Func<DictionaryWindow> dictionaryWindowFactory,
        Func<SettingsWindow> settingsWindowFactory,
        IMessenger messenger)
    {
        InitializeComponent();
        DataContext = viewModel;
        _dictionaryWindowFactory = dictionaryWindowFactory;
        _settingsWindowFactory = settingsWindowFactory;
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

    public void Receive(SettingsRequestedMessage message)
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        // 窗口持有独立的 SettingsViewModel（Loaded 时从设置库加载当前值），
        // 修改经 SettingsAppliedMessage 广播回本窗口，由 Receive 同步置顶/主题
        _settingsWindow = _settingsWindowFactory();
        _settingsWindow.Show();
    }

    public void Receive(SettingsAppliedMessage message)
    {
        // 置顶与主题均为视图层行为，由代码后置直接应用
        Topmost = message.IsTopmost;
        ThemeManager.Apply(ThemeMapper.Map(message.Theme));
    }

    private void OnWindowDrag(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
