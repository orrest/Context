using Microsoft.Extensions.DependencyInjection;

namespace Context.App;

public static class DependencyInjection
{
    public static IServiceCollection AddWindows(this IServiceCollection services)
    {
        services.AddTransient<DictionaryWindow>();
        services.AddSingleton<Func<DictionaryWindow>>(
            sp => () => sp.GetRequiredService<DictionaryWindow>());

        services.AddTransient<SettingsWindow>();
        services.AddSingleton<Func<SettingsWindow>>(
            sp => () => sp.GetRequiredService<SettingsWindow>());

        services.AddTransient<MainWindow>();

        return services;
    }
}
