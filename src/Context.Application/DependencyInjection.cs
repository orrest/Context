using Context.Application.Abstractions;
using Context.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ProductService>();
        return services;
    }
}
