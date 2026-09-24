using System.Windows;
using Context.Application;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace Context.App;

// 用完全限定名：外层命名空间 Context 下的 Context.Application 项目会让裸的 "Application" 产生歧义。
public partial class App : System.Windows.Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services
            .AddPresentation()
            .AddApplication()
            .AddInfrastructure();

        _services = services.BuildServiceProvider();

        await _services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

        MainWindow = _services.GetRequiredService<MainWindow>();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
