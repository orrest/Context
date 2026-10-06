using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS AppSettings (
                IsTopmost INTEGER NOT NULL DEFAULT 1,
                Theme     TEXT
            );
            """, cancellationToken: cancellationToken));

        // 旧版本库补列（SQLite 不支持 ADD COLUMN IF NOT EXISTS）
        var columns = await connection.QueryAsync(
            new CommandDefinition("PRAGMA table_info(AppSettings);", cancellationToken: cancellationToken));
        if (columns.All(column => column.name != "Theme"))
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "ALTER TABLE AppSettings ADD COLUMN Theme TEXT;",
                cancellationToken: cancellationToken));
        }

        if (columns.All(column => column.name != "GlobalHotkey"))
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "ALTER TABLE AppSettings ADD COLUMN GlobalHotkey TEXT;",
                cancellationToken: cancellationToken));
        }

        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS hotkey_bindings (
                CommandId  TEXT PRIMARY KEY,
                KeyGesture TEXT NOT NULL
            );
            """, cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS plugin_settings (
                Id    TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
            """, cancellationToken: cancellationToken));
    }
}
