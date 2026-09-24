using Context.UseCases.Abstractions;
using Context.UseCases.Services;
using Context.Domain.Entities;
using Context.Presentation.ViewModels;
using NSubstitute;

namespace Context.Tests;

public class MainViewModelTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _viewModel = new MainViewModel(new ProductService(_repository));
    }

    [Fact]
    public async Task LoadCommand_PopulatesProducts()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Product> { new() { Id = 1, Name = "显示器" } });

        await _viewModel.LoadCommand.ExecuteAsync(null);

        var item = Assert.Single(_viewModel.Products);
        Assert.Equal("显示器", item.Name);
    }

    [Fact]
    public async Task AddCommand_AddsProduct_AndClearsInput()
    {
        _viewModel.NewProductName = "  耳机  ";
        Product? added = null;
        await _repository.AddAsync(Arg.Do<Product>(p => added = p), Arg.Any<CancellationToken>());

        await _viewModel.AddCommand.ExecuteAsync(null);

        Assert.NotNull(added);
        Assert.Equal("耳机", added!.Name);
        var item = Assert.Single(_viewModel.Products);
        Assert.Equal("耳机", item.Name);
        Assert.Equal(string.Empty, _viewModel.NewProductName);
    }

    [Fact]
    public async Task AddCommand_DoesNothing_WhenNameIsBlank()
    {
        _viewModel.NewProductName = "   ";

        await _viewModel.AddCommand.ExecuteAsync(null);

        await _repository.DidNotReceive().AddAsync(
            Arg.Any<Product>(), Arg.Any<CancellationToken>());
        Assert.Empty(_viewModel.Products);
    }
}
