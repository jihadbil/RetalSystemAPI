using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.Core.Auth;
using RetalSystemAPI.MAUI.Services;

namespace RetalSystemAPI.MAUI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthStateService _authStateService;
    private readonly IProductApiService _productApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly ICategoryApiService _categoryApiService;

    [ObservableProperty]
    private int _productsCount;

    [ObservableProperty]
    private int _branchesCount;

    [ObservableProperty]
    private int _categoriesCount;

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(
        IAuthStateService authStateService,
        IProductApiService productApiService,
        IBranchApiService branchApiService,
        ICategoryApiService categoryApiService)
    {
        _authStateService = authStateService;
        _productApiService = productApiService;
        _branchApiService = branchApiService;
        _categoryApiService = categoryApiService;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsBusy = true;

        var productsRes = await _productApiService.GetPagedAsync(1, 1);
        if (productsRes.Success && productsRes.Data != null)
        {
            ProductsCount = productsRes.Data.TotalCount;
        }

        var branchesRes = await _branchApiService.GetAllAsync();
        if (branchesRes.Success && branchesRes.Data != null)
        {
            BranchesCount = branchesRes.Data.Count;
        }

        var categoriesRes = await _categoryApiService.GetAllAsync();
        if (categoriesRes.Success && categoriesRes.Data != null)
        {
            CategoriesCount = categoriesRes.Data.Count;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authStateService.ClearTokenAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
