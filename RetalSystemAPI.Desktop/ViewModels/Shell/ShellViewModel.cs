using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.ViewModels.Base;
using RetalSystemAPI.Desktop.Views.Branches;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Dashboard;
using RetalSystemAPI.Desktop.Views.Purchase;
using RetalSystemAPI.Desktop.Views.Stock;
using RetalSystemAPI.Desktop.Views.Suppliers;
using RetalSystemAPI.Desktop.Views.Tenants;
using RetalSystemAPI.Desktop.Views.Warehouses;

namespace RetalSystemAPI.Desktop.ViewModels.Shell;

public partial class ShellViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly AuthStateService _authStateService;

    [ObservableProperty]
    private UserControl? _currentView;

    [ObservableProperty]
    private string _currentTitle = "الرئيسية";

    public ShellViewModel(INavigationService navigationService, AuthStateService authStateService)
    {
        _navigationService = navigationService;
        _authStateService = authStateService;

        _navigationService.Navigated += (s, view) => CurrentView = view;
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentTitle = "لوحة التحكم";
        _navigationService.NavigateTo<DashboardView>();
    }

    [RelayCommand]
    private void NavigateToTenants()
    {
        CurrentTitle = "الملف الشخصي للمستأجر";
        _navigationService.NavigateTo<TenantsView>();
    }

    [RelayCommand]
    private void NavigateToBranches()
    {
        CurrentTitle = "الفروع";
        _navigationService.NavigateTo<BranchesView>();
    }

    [RelayCommand]
    private void NavigateToCategories()
    {
        CurrentTitle = "التصنيفات";
        _navigationService.NavigateTo<CategoriesView>();
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        CurrentTitle = "المنتجات";
        _navigationService.NavigateTo<ProductsView>();
    }

    [RelayCommand]
    private void NavigateToUnits()
    {
        CurrentTitle = "وحدات القياس";
        _navigationService.NavigateTo<UnitsView>();
    }

    [RelayCommand]
    private void NavigateToSuppliers()
    {
        CurrentTitle = "الموردين";
        _navigationService.NavigateTo<SuppliersView>();
    }

    [RelayCommand]
    private void NavigateToWarehouses()
    {
        CurrentTitle = "المخازن وصالات العرض";
        _navigationService.NavigateTo<WarehousesView>();
    }

    [RelayCommand]
    private void NavigateToStock()
    {
        CurrentTitle = "إدارة المخزون والتسوية";
        _navigationService.NavigateTo<StockView>();
    }

    [RelayCommand]
    private void NavigateToPurchaseOrders()
    {
        CurrentTitle = "الطلبيات والمشتريات";
        _navigationService.NavigateTo<PurchaseOrdersView>();
    }

    [RelayCommand]
    private void NavigateToUsers()
    {
        CurrentTitle = "إدارة المستخدمين والصلاحيات";
        _navigationService.NavigateTo<RetalSystemAPI.Desktop.Views.Users.UsersView>();
    }

    [RelayCommand]
    private void Logout()
    {
        _authStateService.ClearToken();
        _navigationService.NavigateToLogin();
    }
}
