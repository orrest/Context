using Context.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
        services.AddTransient<PetViewModel>();
        services.AddTransient<WallpaperViewModel>();
        services.AddSingleton<Func<PetViewModel>>(
            sp => () => sp.GetRequiredService<PetViewModel>());
        services.AddSingleton<Func<WallpaperViewModel>>(
            sp => () => sp.GetRequiredService<WallpaperViewModel>());
        return services;
    }
}
