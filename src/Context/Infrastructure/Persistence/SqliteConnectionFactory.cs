using System.Data;
using Microsoft.Data.Sqlite;

namespace Context.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
