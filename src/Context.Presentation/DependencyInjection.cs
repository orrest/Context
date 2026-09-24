using Context.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        return services;
    }
}
