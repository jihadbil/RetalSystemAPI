using System;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Product;

namespace RetalSystemAPI.MAUI.Views.Catalog;

public partial class ProductsPage : ContentPage
{
    public ProductsPage(ProductsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProductsViewModel vm)
        {
            await vm.LoadCategoriesCommand.ExecuteAsync(null);
            await vm.LoadProductsCommand.ExecuteAsync(null);
        }
    }

    private async void OnProductTapped(object sender, EventArgs e)
    {
        if (sender is Element element && element.BindingContext is ProductResponseDto product)
        {
            var parameters = new System.Collections.Generic.Dictionary<string, object>
            {
                { "ProductId", product.Id }
            };
            await Shell.Current.GoToAsync("ProductDetailPage", parameters);
        }
    }
}
