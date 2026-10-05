using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Context.App.Commands;
using Plugin;
using Context.Presentation.Messages;
using Context.UseCases.Abstractions;
using Orrest.Ui.Themes;
using Slime.Views;

namespace Context.App.Views;

/// <summary>
/// 主窗口：宿主的透明占位窗体（无边框、不进任务栏，随应用生命周期常驻），
/// 承载窗口级行为——托盘入口、拖动、应用内快捷键；其内容由桌宠插件提供
/// （构造注入 <see cref="SlimeView"/>），主窗口本身不包含桌宠实现。
/// 应用内快捷键（KeyBinding）由本窗口内联装配：主窗口是唯一常驻窗口，
/// 生效手势 = 显式绑定 ?? 命令声明默认；设置页变更经
/// <see cref="HotkeysChangedMessage"/> 通知本窗口重装。
/// </summary>
public partial class MainWindow : Window,
    IRecipient<SettingsAppliedMessage>,
    IRecipient<HotkeysChangedMessage>
{
    private readonly IMessenger _messenger;
    private readonly IHotkeyCommandCatalog _hotkeyCommandCatalog;
    private readonly IHotkeyBindingsRepository _hotkeyBindingsRepository;
    private readonly KeyGestureConverter _gestureConverter = new();

    public MainWindow(
        SlimeView slimeView,
        IHotkeyCommandCatalog hotkeyCommandCatalog,
        IHotkeyBindingsRepository hotkeyBindingsRepository,
        IMessenger messenger)
    {
        InitializeComponent();
        _messenger = messenger;
        _hotkeyCommandCatalog = hotkeyCommandCatalog;
        _hotkeyBindingsRepository = hotkeyBindingsRepository;

        // 内容为桌宠插件提供的视图；透明画布保持窗口整体可命中、可拖动
        Content = slimeView;

        // 托盘菜单命令取自命令目录：与对应应用内快捷键共用同一命令实例
        SettingsMenuItem.Command = hotkeyCommandCatalog.GetCommands()
            .FirstOrDefault(c => c.Id == SettingsCommands.OpenCommandId)?.Command;

        // 主窗口是唯一常驻窗口：应用内快捷键仅挂载于本窗口（窗口显示前装配即可生效）
        _ = ApplyInputBindingsAsync();

        _messenger.RegisterAll(this);
        Closed += (_, _) => _messenger.UnregisterAll(this);
    }

    public void Receive(SettingsAppliedMessage message)
    {
        // 置顶与主题均为视图层行为，由代码后置直接应用
        Topmost = message.IsTopmost;
        ThemeManager.Apply(ThemeMapper.Map(message.Theme));
    }

    /// <summary>接收快捷键变更消息，重装本窗口绑定。</summary>
    public void Receive(HotkeysChangedMessage message)
    {
        _ = ApplyInputBindingsAsync();
    }

    /// <summary>
    /// 依据命令目录与持久化绑定重建本窗口的 InputBindings。
    /// 本窗口 InputBindings 仅有此处装配的内容，整体清装安全。
    /// </summary>
    private async Task ApplyInputBindingsAsync(CancellationToken cancellationToken = default)
    {
        var commands = _hotkeyCommandCatalog.GetCommands();
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
