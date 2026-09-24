using Context.Application.Abstractions;
using Context.Application.Services;
using Context.Domain.Entities;
using NSubstitute;

namespace Context.Tests;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repository);
    }

    [Fact]
    public async Task AddAsync_TrimsName_AndAddsProduct()
    {
        var product = await _service.AddAsync("  键盘  ");

        Assert.Equal("键盘", product.Name);
        await _repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "键盘"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_Throws_WhenNameIsWhitespace()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync("   "));

        await _repository.DidNotReceive().AddAsync(
            Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProductsFromRepository()
    {
        var expected = new List<Product> { new() { Id = 1, Name = "鼠标" } };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _service.GetAllAsync();

        Assert.Same(expected, result);
    }
}
