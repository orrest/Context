using Context.Domain.Entities;

namespace Context.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
