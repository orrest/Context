using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Context.App.Views;
using Plugin;

namespace Context.App.Commands;

/// <summary>
/// 宿主设置功能的命令入口：与插件命令（DictionaryCommands / WallpaperCommands）同一模式——
/// 以 <see cref="HotkeyCommandAttribute"/> 声明命令（id / 展示名 / 默认键手势），经 DI 注册为
/// <see cref="ICommandProvider"/> 后由宿主命令目录隐式聚合。
/// 单例注册：命令实例被常驻目录聚合，故不持有窗口引用——可见窗口经
/// Application.Current.Windows 动态查询（可见则激活，否则经工厂新建弹出）。
/// </summary>
public sealed class SettingsCommands : ICommandProvider
{
    public const string OpenCommandId = "settings.open";

    private readonly Func<SettingsWindow> _settingsWindowFactory;

    [HotkeyCommand(OpenCommandId, "设置：打开设置窗口", "Alt+M")]
    public IRelayCommand OpenCommand { get; }

    public SettingsCommands(Func<SettingsWindow> settingsWindowFactory)
    {
        _settingsWindowFactory = settingsWindowFactory;
        OpenCommand = new RelayCommand(OpenWindow);
    }

    private void OpenWindow()
    {
        var existing = Application.Current?.Windows
            .OfType<SettingsWindow>()
            .FirstOrDefault(window => window.IsVisible);
        if (existing is not null)
        {
            existing.Activate();
            return;
        }

        _settingsWindowFactory().Show();
    }
}
