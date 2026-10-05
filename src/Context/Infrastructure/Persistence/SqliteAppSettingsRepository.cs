using Context.Domain.Entities;
using Context.UseCases.Abstractions;
using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class SqliteAppSettingsRepository(IDbConnectionFactory connectionFactory) : IAppSettingsRepository
{
    public async Task<AppSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AppSettingsRow>(new CommandDefinition("""
            SELECT IsTopmost, Theme FROM AppSettings LIMIT 1;
            """, cancellationToken: cancellationToken));
        if (row is null)
        {
            // 首次运行：空表返回默认值（置顶开启、浅色主题）
            return new AppSettings();
        }

        return new AppSettings
        {
            IsTopmost = row.IsTopmost != 0,
            Theme = row.Theme,
        };
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition("""
            DELETE FROM AppSettings;
            INSERT INTO AppSettings (IsTopmost, Theme) VALUES (@IsTopmost, @Theme);
            """, new { settings.IsTopmost, settings.Theme }, cancellationToken: cancellationToken));
    }

    /// <summary>查询行映射（SQLite INTEGER 物化为 long，Dapper 构造器映射不做类型转换）。</summary>
    private sealed record AppSettingsRow(long IsTopmost, string Theme);
}
