using System.Windows;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Context.UseCases;
using Context.UseCases.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orrest.Ui.Themes;

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
            .AddUseCases()
            .AddInfrastructure()
            .AddWindows();

        _services = services.BuildServiceProvider();

        await _services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

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

