using CommunityToolkit.Mvvm.Messaging;
using Context.Presentation.Services;
using Context.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<HotkeyItemViewModel>();
        services.AddSingleton<Func<HotkeyItemViewModel>>(
            sp => () => sp.GetRequiredService<HotkeyItemViewModel>());
        services.AddTransient<SettingItemViewModel>();
        services.AddSingleton<Func<SettingItemViewModel>>(
            sp => () => sp.GetRequiredService<SettingItemViewModel>());
        services.AddSingleton<PluginSettingsApplier>();

        return services;
    }
}
