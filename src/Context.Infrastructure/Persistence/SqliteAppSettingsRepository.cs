using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class SqliteAppSettingsRepository(IDbConnectionFactory connectionFactory) : IAppSettingsRepository
{
    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var isTopmost = await connection.ExecuteScalarAsync<bool?>(new CommandDefinition("""
            SELECT IsTopmost FROM AppSettings LIMIT 1;
            """, cancellationToken: cancellationToken));
        return new AppSettings { IsTopmost = isTopmost ?? true };
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM AppSettings;
            INSERT INTO AppSettings (IsTopmost) VALUES (@IsTopmost);
            """, settings, cancellationToken: cancellationToken));
    }
}
