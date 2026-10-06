using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using Context.App.Commands;
using Plugin;
using Context.Presentation.Messages;
using Context.UseCases.Abstractions;
using Orrest.Ui.Hotkey;
using Orrest.Ui.Themes;
using Slime.Views;

namespace Context.App.Views;

/// <summary>
/// 主窗口：宿主的透明占位窗体（无边框、不进任务栏，随应用生命周期常驻），
/// 承载窗口级行为——托盘入口、拖动、应用内快捷键、全局呼出热键；其内容由桌宠插件提供
/// （构造注入 <see cref="SlimeView"/>），并以插件覆盖层（<see cref="IMainWindowOverlay"/>）
/// 叠加各插件贡献的视图，主窗口本身不包含桌宠或插件实现。
/// 应用内快捷键（KeyBinding）由本窗口内联装配：主窗口是唯一常驻窗口，
/// 生效手势 = 显式绑定 ?? 命令声明默认；设置页变更经
/// <see cref="HotkeysChangedMessage"/> 通知本窗口重装。
/// 全局呼出热键（系统级 RegisterHotKey）同样由本窗口承载：手势取自应用设置，
/// 变更经 <see cref="SettingsAppliedMessage"/> 通知本窗口重注册，触发即呼出本窗口。
/// </summary>
public partial class MainWindow : Window,
    IRecipient<SettingsAppliedMessage>,
    IRecipient<HotkeysChangedMessage>
{
    private readonly IMessenger _messenger;
    private readonly IPluginCatalog _pluginCatalog;
    private readonly IHotkeyBindingsRepository _hotkeyBindingsRepository;
    private readonly IAppSettingsRepository _appSettingsRepository;
    private readonly KeyGestureConverter _gestureConverter = new();
    private readonly GlobalHotkeyController _globalHotkey;
    private readonly SlimeView _slimeView;

    public MainWindow(
        SlimeView slimeView,
        IPluginCatalog pluginCatalog,
        IHotkeyBindingsRepository hotkeyBindingsRepository,
        IAppSettingsRepository appSettingsRepository,
        IEnumerable<IMainWindowOverlay> mainWindowOverlays,
        IMessenger messenger)
    {
        InitializeComponent();
        _messenger = messenger;
        _pluginCatalog = pluginCatalog;
        _hotkeyBindingsRepository = hotkeyBindingsRepository;
        _appSettingsRepository = appSettingsRepository;

        // 内容为桌宠插件提供的视图；透明画布保持窗口整体可命中、可拖动，
        // 插件覆盖层依次叠加其上，显隐由各视图自治（如音乐“喜欢”按钮仅播放时可见）
        _slimeView = slimeView;
        var root = new Grid { Background = Brushes.Transparent };
        root.Children.Add(slimeView);
        foreach (var overlay in mainWindowOverlays)
        {
            root.Children.Add(overlay.View);
        }

        Content = root;

        // 托盘菜单命令取自插件目录：与对应应用内快捷键共用同一命令实例
        SettingsMenuItem.Command = pluginCatalog.GetCommands()
            .FirstOrDefault(c => c.Id == SettingsCommands.OpenCommandId)?.Command;

        // 全局呼出热键：触发即置前并激活常驻主窗口；初值取自应用设置
        _globalHotkey = new GlobalHotkeyController(this);
        _globalHotkey.Pressed += (_, _) => Summon();

        // 主窗口是唯一常驻窗口：应用内快捷键仅挂载于本窗口（窗口显示前装配即可生效）
        _ = ApplyInputBindingsAsync();
        _ = ApplyGlobalHotkeyAsync();

        _messenger.RegisterAll(this);
        Closed += (_, _) =>
        {
            _messenger.UnregisterAll(this);
            _globalHotkey.Dispose();
        };
    }

    public void Receive(SettingsAppliedMessage message)
    {
        // 置顶与主题均为视图层行为，由代码后置直接应用；全局热键随设置重注册
        Topmost = message.IsTopmost;
        ThemeManager.Apply(ThemeMapper.Map(message.Theme));
        _globalHotkey.Apply(message.GlobalHotkey);
    }

    /// <summary>接收快捷键变更消息，重装本窗口绑定。</summary>
    public void Receive(HotkeysChangedMessage message)
    {
        _ = ApplyInputBindingsAsync();
    }

    /// <summary>全局呼出热键触发：主窗口置前并激活，桌宠动画循环播放一段时长。</summary>
    private void Summon()
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        _slimeView.Play();
    }

    /// <summary>从应用设置读取全局呼出热键并注册。</summary>
    private async Task ApplyGlobalHotkeyAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _appSettingsRepository.GetAsync(cancellationToken);
        _globalHotkey.Apply(settings.GlobalHotkey);
    }

    /// <summary>
    /// 依据插件目录与持久化绑定重建本窗口的 InputBindings。
    /// 本窗口 InputBindings 仅有此处装配的内容，整体清装安全。
    /// </summary>
    private async Task ApplyInputBindingsAsync(CancellationToken cancellationToken = default)
    {
        var commands = _pluginCatalog.GetCommands();
        var bindings = await _hotkeyBindingsRepository.GetAllAsync(cancellationToken);
        var bound = bindings.ToDictionary(binding => binding.CommandId);

        var keyBindings = new List<KeyBinding>();
        foreach (var command in commands)
        {
            var gestureText = bound.TryGetValue(command.Id, out var binding)
                ? binding.KeyGesture
                : command.DefaultKeyGesture;
            var gesture = ParseGesture(gestureText)
                // 显式绑定无效时回退命令默认手势（默认亦无效则跳过该命令）
                ?? ParseGesture(command.DefaultKeyGesture);
            if (gesture is null)
            {
                continue;
            }

            keyBindings.Add(new KeyBinding(command.Command, gesture));
        }

        InputBindings.Clear();
        foreach (var keyBinding in keyBindings)
        {
            InputBindings.Add(keyBinding);
        }
    }

    private KeyGesture? ParseGesture(string gestureText)
    {
        try
        {
            return _gestureConverter.ConvertFromString(gestureText) as KeyGesture;
        }
        catch (FormatException)
        {
            // 无效手势串静默跳过，由调用方回退默认
            return null;
        }
    }

    private void OnWindowDrag(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
