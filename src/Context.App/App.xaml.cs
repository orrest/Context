using System.Windows;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Context.UseCases;
using Microsoft.Extensions.DependencyInjection;

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
            .AddInfrastructure();
        services.AddTransient<DictionaryWindow>();
        services.AddSingleton<Func<DictionaryWindow>>(
            sp => () => sp.GetRequiredService<DictionaryWindow>());
        services.AddTransient<MainWindow>();

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
