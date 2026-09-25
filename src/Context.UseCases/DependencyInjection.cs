using Context.UseCases.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Context.UseCases;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddSingleton<WallpaperService>();
        return services;
    }
}
