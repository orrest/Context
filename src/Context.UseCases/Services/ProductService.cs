using Context.UseCases.Abstractions;
using Context.Domain.Entities;

namespace Context.UseCases.Services;

public sealed class ProductService(IProductRepository repository)
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await repository.GetAllAsync(cancellationToken);

    public async Task<Product> AddAsync(string name, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("产品名称不能为空。", nameof(name));
        }

        var product = new Product { Name = trimmed };
        await repository.AddAsync(product, cancellationToken);
        return product;
    }
}
