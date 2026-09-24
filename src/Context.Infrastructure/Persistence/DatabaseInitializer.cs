using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS Products (
                Id   INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );
            """, cancellationToken: cancellationToken));
    }
}
