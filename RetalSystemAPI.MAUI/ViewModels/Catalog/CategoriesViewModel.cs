using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Catalog.Category;

namespace RetalSystemAPI.MAUI.ViewModels.Catalog;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly ICategoryApiService _categoryApiService;

    [ObservableProperty]
    private ObservableCollection<CategoryResponseDto> _categories = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public CategoriesViewModel(ICategoryApiService categoryApiService)
    {
        _categoryApiService = categoryApiService;
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await _categoryApiService.GetAllAsync();
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Categories = new ObservableCollection<CategoryResponseDto>(result.Data);
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل التصنيفات";
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(CategoryResponseDto category)
    {
        if (category == null) return;

        var result = await _categoryApiService.ToggleActiveStatusAsync(category.Id);
        if (result.Success)
        {
            await LoadCategoriesAsync();
        }
    }
}
