using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class SqliteHotkeyBindingsRepository(IDbConnectionFactory connectionFactory) : IHotkeyBindingsRepository
{
    public async Task<IReadOnlyList<HotkeyBinding>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var bindings = await connection.QueryAsync<HotkeyBinding>(new CommandDefinition("""
            SELECT CommandId, KeyGesture FROM hotkey_bindings ORDER BY CommandId;
            """, cancellationToken: cancellationToken));
        return [.. bindings];
    }

    public async Task SaveAsync(HotkeyBinding binding, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            INSERT INTO hotkey_bindings (CommandId, KeyGesture) VALUES (@CommandId, @KeyGesture)
            ON CONFLICT(CommandId) DO UPDATE SET KeyGesture = excluded.KeyGesture;
            """, new { binding.CommandId, binding.KeyGesture }, cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(string commandId, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM hotkey_bindings WHERE CommandId = @commandId;",
            new { commandId }, cancellationToken: cancellationToken));
    }
}
