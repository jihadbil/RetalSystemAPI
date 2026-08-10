using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Models.DTOs.Catalog.Product;

namespace RetalSystemAPI.MAUI.ViewModels.Catalog;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductApiService _productApiService;
    private readonly ICategoryApiService _categoryApiService;

    [ObservableProperty]
    private ObservableCollection<ProductResponseDto> _products = new();

    [ObservableProperty]
    private ObservableCollection<CategoryResponseDto> _categories = new();

    [ObservableProperty]
    private CategoryResponseDto? _selectedCategory;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public ProductsViewModel(IProductApiService productApiService, ICategoryApiService categoryApiService)
    {
        _productApiService = productApiService;
        _categoryApiService = categoryApiService;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryApiService.GetAllAsync();
        if (result.Success && result.Data != null)
        {
            Categories = new ObservableCollection<CategoryResponseDto>(result.Data);
        }
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        Guid? catId = SelectedCategory?.Id;
        var result = await _productApiService.GetPagedAsync(CurrentPage, 10, catId);
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Products = new ObservableCollection<ProductResponseDto>(result.Data.Items);
            CurrentPage = result.Data.PageNumber;
            TotalPages = result.Data.TotalPages;
            TotalCount = result.Data.TotalCount;
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل المنتجات";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadProductsAsync();
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        var result = await _productApiService.SearchAsync(SearchQuery);
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Products = new ObservableCollection<ProductResponseDto>(result.Data);
            TotalCount = result.Data.Count;
        }
        else
        {
            ErrorMessage = result.Message ?? "لم يتم العثور على نتائج للبحث";
        }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        IsBusy = true;
        var bytes = await _productApiService.ExportExcelAsync();
        IsBusy = false;

        if (bytes != null && bytes.Length > 0)
        {
            // Save or alert user
            await Shell.Current.DisplayAlert("تصدير Excel", "تم تصدير كافة بيانات الأصناف والباركودات والتصنيفات بنجاح", "تم");
        }
        else
        {
            await Shell.Current.DisplayAlert("خطأ", "فشل تصدير ملف Excel", "موافق");
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        IsBusy = true;
        var bytes = await _productApiService.DownloadTemplateAsync();
        IsBusy = false;

        if (bytes != null && bytes.Length > 0)
        {
            await Shell.Current.DisplayAlert("قالب Excel", "تم تنزيل قالب الاستيراد التوضيحي بنجاح", "تم");
        }
        else
        {
            await Shell.Current.DisplayAlert("خطأ", "فشل تنزيل قالب Excel", "موافق");
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProductsAsync();
        }
    }
}
