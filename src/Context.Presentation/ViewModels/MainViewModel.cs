using System.Collections.ObjectModel;
using Context.UseCases.Services;
using Context.Domain.Entities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Context.Presentation.ViewModels;

public partial class MainViewModel(ProductService productService) : ObservableObject
{
    public ObservableCollection<Product> Products { get; } = [];

    [ObservableProperty]
    private string newProductName = string.Empty;

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        Products.Clear();
        foreach (var product in await productService.GetAllAsync(cancellationToken))
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    private async Task AddAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewProductName))
        {
            return;
        }

        var product = await productService.AddAsync(NewProductName, cancellationToken);
        Products.Add(product);
        NewProductName = string.Empty;
    }
}
