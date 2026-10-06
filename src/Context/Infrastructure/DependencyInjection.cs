using Context.UseCases.Abstractions;
using Context.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Context.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString = "Data Source=app.db")
    {
        services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<IAppSettingsRepository, SqliteAppSettingsRepository>();
        services.AddSingleton<IHotkeyBindingsRepository, SqliteHotkeyBindingsRepository>();
        services.AddSingleton<IPluginSettingsRepository, SqlitePluginSettingsRepository>();

        return services;
    }
}
