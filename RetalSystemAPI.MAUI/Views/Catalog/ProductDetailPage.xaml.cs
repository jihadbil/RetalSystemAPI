using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Catalog;

namespace RetalSystemAPI.MAUI.Views.Catalog;

public partial class ProductDetailPage : ContentPage, IQueryAttributable
{
    private readonly ProductDetailViewModel _viewModel;

    public ProductDetailPage(ProductDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ProductId", out var productIdObj) && productIdObj is Guid productId)
        {
            await _viewModel.LoadProductDetailsAsync(productId);
        }
    }
}
