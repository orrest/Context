using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class SqlitePluginSettingsRepository(IDbConnectionFactory connectionFactory) : IPluginSettingsRepository
{
    public async Task<IReadOnlyList<PluginSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var settings = await connection.QueryAsync<PluginSetting>(new CommandDefinition("""
            SELECT Id, Value FROM plugin_settings ORDER BY Id;
            """, cancellationToken: cancellationToken));
        return [.. settings];
    }

    public async Task SaveAsync(PluginSetting setting, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO plugin_settings (Id, Value) VALUES (@Id, @Value)
            ON CONFLICT(Id) DO UPDATE SET Value = excluded.Value;
            """, setting, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM plugin_settings WHERE Id = @id;",
            new { id }, cancellationToken: cancellationToken));
    }
}
