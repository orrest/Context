using System.Windows;
using Context.App.Views;
using Context.Presentation.Services;
using Dictionary;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Context.UseCases.Abstractions;
using Music;
using Wallpaper;
using Microsoft.Extensions.DependencyInjection;
using Orrest.Ui.Themes;
using Slime;

namespace Context.App;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services
            .AddPresentation()
            .AddInfrastructure()
            .AddWindows()
            .AddHotkeys()
            .AddDictionary()
            .AddWallpaper()
            .AddMusic()
            .AddSlime();

        _services = services.BuildServiceProvider();

        await _services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
        // 插件设置在主窗口装配前回写提供者属性（提供者属性随取随读，此处保证初值就绪）
        await _services.GetRequiredService<PluginSettingsApplier>().ApplyAllAsync();

        // 应用已保存的主题与置顶（窗口显示前生效，避免闪变）
        var settings = await _services.GetRequiredService<IAppSettingsRepository>().GetAsync();
        ThemeManager.Apply(ThemeMapper.Map(settings.Theme));
        MainWindow = _services.GetRequiredService<MainWindow>();
        MainWindow.Topmost = settings.IsTopmost;
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
