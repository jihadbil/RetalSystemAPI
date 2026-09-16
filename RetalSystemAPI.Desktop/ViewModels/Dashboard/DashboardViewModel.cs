using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Dashboard;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;
using RetalSystemAPI.Desktop.Views.Branches;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Customers;
using RetalSystemAPI.Desktop.Views.Pos;
using RetalSystemAPI.Desktop.Views.Purchase;
using RetalSystemAPI.Desktop.Views.Sales;
using RetalSystemAPI.Desktop.Views.Stock;
using RetalSystemAPI.Desktop.Views.Suppliers;

namespace RetalSystemAPI.Desktop.ViewModels.Dashboard;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardApiService _dashboardApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private DashboardKpiDto _kpis = new();

    [ObservableProperty]
    private DashboardCountsDto _counts = new();

    [ObservableProperty]
    private ObservableCollection<DailySalesPointDto> _dailySalesTrend = new();

    [ObservableProperty]
    private ObservableCollection<TopSellingProductDto> _topSellingProducts = new();

    [ObservableProperty]
    private ObservableCollection<LowStockItemDto> _lowStockAlerts = new();

    [ObservableProperty]
    private ObservableCollection<RecentInvoiceDto> _recentInvoices = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private bool _hasLowStock;

    [ObservableProperty]
    private bool _hasTopProducts;

    [ObservableProperty]
    private bool _hasRecentInvoices;

    [ObservableProperty]
    private string _lastUpdatedText = "جاري التحديث...";

    public DashboardViewModel(
        IDashboardApiService dashboardApiService,
        IBranchApiService branchApiService,
        INavigationService navigationService,
        IToastService toastService)
    {
        _dashboardApiService = dashboardApiService;
        _branchApiService = branchApiService;
        _navigationService = navigationService;
        _toastService = toastService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAsync();
        await LoadDashboardAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (await LoadDashboardAsync())
            _toastService.ShowSuccess("تم تحديث بيانات لوحة التحكم بنجاح", "لوحة التحكم");
    }

    private async Task LoadBranchesAsync()
    {
        try
        {
            var res = await _branchApiService.GetAllAsync();
            if (res.Success && res.Data != null)
            {
                Branches.Clear();
                // إضافة خيار "كافة الفروع"
                Branches.Add(new BranchDto { Id = Guid.Empty, Name = "🏢 كافة الفروع" });
                foreach (var b in res.Data)
                {
                    Branches.Add(b);
                }
                SelectedBranch = Branches.FirstOrDefault();
            }
        }
        catch
        {
            // تجاهل خطأ جلب الفروع واستمرار تحميل الداشبورد
        }
    }

    private int _loadVersion;
    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        _ = LoadDashboardAsync();
    }

    public async Task<bool> LoadDashboardAsync()
    {
        var loaded = false;
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            Guid? branchId = SelectedBranch != null && SelectedBranch.Id != Guid.Empty
                ? SelectedBranch.Id
                : null;

            var res = await _dashboardApiService.GetSummaryAsync(branchId);
            if (version != _loadVersion) return;
            if (res.Success && res.Data != null)
            {
                var data = res.Data;
                Kpis = data.Kpis;
                Counts = data.Counts;

                // 1. حساب ارتفاعات أعمدة الرسم البياني التفاعلي
                decimal maxSale = data.DailySalesTrend.Count > 0 ? data.DailySalesTrend.Max(x => x.SalesAmount) : 0;
                if (maxSale <= 0) maxSale = 1;

                DailySalesTrend.Clear();
                foreach (var point in data.DailySalesTrend)
                {
                    // حساب النسبة بين 15% و 100% لضمان ظهور العمود حتى للقيم الصغيرة
                    double percentage = (double)(point.SalesAmount / maxSale);
                    point.BarHeightPercentage = Math.Max(0, percentage * 120);
                    DailySalesTrend.Add(point);
                }

                // 2. الأصناف الأكثر مبيعاً
                TopSellingProducts.Clear();
                foreach (var p in data.TopSellingProducts)
                {
                    TopSellingProducts.Add(p);
                }
                HasTopProducts = TopSellingProducts.Count > 0;

                // 3. رادار النواقص
                LowStockAlerts.Clear();
                foreach (var item in data.LowStockAlerts)
                {
                    LowStockAlerts.Add(item);
                }
                HasLowStock = LowStockAlerts.Count > 0;

                // 4. آخر الفواتير
                RecentInvoices.Clear();
                foreach (var inv in data.RecentInvoices)
                {
                    RecentInvoices.Add(inv);
                }
                HasRecentInvoices = RecentInvoices.Count > 0;

                LastUpdatedText = $"آخر تحديث: {DateTime.Now:hh:mm:ss tt}";
                loaded = true;
            }
            else
            {
                _toastService.ShowError(res.Message ?? "فشل في جلب بيانات لوحة التحكم", "خطأ في الاتصال");
            }
        });
        return loaded;
    }

    // ── أوامر التنقل السريع ────────────────────────────────────────

    [RelayCommand]
    private void NavigateToPos() => _navigationService.NavigateTo<PosView>();

    [RelayCommand]
    private void NavigateToProducts() => _navigationService.NavigateTo<ProductsView>();

    [RelayCommand]
    private void NavigateToCategories() => _navigationService.NavigateTo<CategoriesView>();

    [RelayCommand]
    private void NavigateToBranches() => _navigationService.NavigateTo<BranchesView>();

    [RelayCommand]
    private void NavigateToCustomers() => _navigationService.NavigateTo<CustomersView>();

    [RelayCommand]
    private void NavigateToSuppliers() => _navigationService.NavigateTo<SuppliersView>();

    [RelayCommand]
    private void NavigateToSalesInvoices() => _navigationService.NavigateTo<SalesInvoicesView>();

    [RelayCommand]
    private void NavigateToPurchaseInvoices() => _navigationService.NavigateTo<PurchaseInvoicesView>();

    [RelayCommand]
    private void NavigateToPurchaseOrders() => _navigationService.NavigateTo<PurchaseOrdersView>();

    [RelayCommand]
    private void NavigateToStock() => _navigationService.NavigateTo<StockView>();
}
