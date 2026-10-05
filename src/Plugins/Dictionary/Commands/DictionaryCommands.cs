using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Dictionary.Views;
using Plugin;

namespace Dictionary.Commands;

/// <summary>
/// 词典插件对外声明的命令入口：以 <see cref="HotkeyCommandAttribute"/> 声明命令
/// （id / 展示名 / 默认键手势），经 DI 注册为 <see cref="ICommandProvider"/> 后
/// 由宿主命令目录隐式聚合——词典库不感知任何快捷键机制。
/// 单例注册：命令实例被常驻目录聚合，故不持有窗口引用——可见窗口经
/// Application.Current.Windows 动态查询（可见则激活，否则经工厂新建弹出）。
/// </summary>
public sealed class DictionaryCommands : ICommandProvider
{
    public const string OpenCommandId = "dictionary.open";

    private readonly Func<DictionaryWindow> _dictionaryWindowFactory;

    [HotkeyCommand(OpenCommandId, "词典：打开词典窗口", "Alt+D")]
    public IRelayCommand OpenCommand { get; }

    public DictionaryCommands(Func<DictionaryWindow> dictionaryWindowFactory)
    {
        _dictionaryWindowFactory = dictionaryWindowFactory;
        OpenCommand = new RelayCommand(OpenWindow);
    }

    private void OpenWindow()
    {
        var existing = Application.Current?.Windows
            .OfType<DictionaryWindow>()
            .FirstOrDefault(window => window.IsVisible);
        if (existing is not null)
        {
            existing.Activate();
            return;
        }

        _dictionaryWindowFactory().Show();
    }
}
