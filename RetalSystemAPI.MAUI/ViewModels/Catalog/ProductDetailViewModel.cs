using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.MAUI.ViewModels.Catalog;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly IProductApiService _productApiService;

    [ObservableProperty]
    private ProductResponseDto? _product;

    [ObservableProperty]
    private ObservableCollection<ProductUnitResponseDto> _units = new();

    [ObservableProperty]
    private ObservableCollection<ProductBarCodeResponseDto> _barCodes = new();

    [ObservableProperty]
    private ObservableCollection<ProductImageResponseDto> _images = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ProductDetailViewModel(IProductApiService productApiService)
    {
        _productApiService = productApiService;
    }

    public async Task LoadProductDetailsAsync(Guid productId)
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await _productApiService.GetByIdAsync(productId);
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Product = result.Data;
            Units = new ObservableCollection<ProductUnitResponseDto>(result.Data.Units ?? new());
            BarCodes = new ObservableCollection<ProductBarCodeResponseDto>(result.Data.BarCodes ?? new());
            Images = new ObservableCollection<ProductImageResponseDto>(result.Data.Images ?? new());
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل تفاصيل المنتج";
        }
    }
}
