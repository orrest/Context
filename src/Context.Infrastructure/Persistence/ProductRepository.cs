using Context.UseCases.Abstractions;
using Context.Domain.Entities;
using Dapper;

namespace Context.Infrastructure.Persistence;

public sealed class ProductRepository(IDbConnectionFactory connectionFactory) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var products = await connection.QueryAsync<Product>(new CommandDefinition(
            "SELECT Id, Name FROM Products ORDER BY Id;",
            cancellationToken: cancellationToken));
        return products.AsList();
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        product.Id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "INSERT INTO Products (Name) VALUES (@Name); SELECT last_insert_rowid();",
            product,
            cancellationToken: cancellationToken));
    }
}
