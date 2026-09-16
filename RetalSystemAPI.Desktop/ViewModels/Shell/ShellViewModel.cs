using System;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Constants;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;
using RetalSystemAPI.Desktop.Views.Branches;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Customers;
using RetalSystemAPI.Desktop.Views.Dashboard;
using RetalSystemAPI.Desktop.Views.Pos;
using RetalSystemAPI.Desktop.Views.Purchase;
using RetalSystemAPI.Desktop.Views.Sales;
using RetalSystemAPI.Desktop.Views.Stock;
using RetalSystemAPI.Desktop.Views.Suppliers;
using RetalSystemAPI.Desktop.Views.Tenants;
using RetalSystemAPI.Desktop.Views.Warehouses;

namespace RetalSystemAPI.Desktop.ViewModels.Shell;

public partial class ShellViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly AuthStateService _authStateService;
    private readonly IThemeService _themeService;
    public IToastService ToastService { get; }

    [ObservableProperty]
    private UserControl? _currentView;

    [ObservableProperty]
    private string _currentTitle = "لوحة التحكم";

    [ObservableProperty]
    private string _activeSection = "Dashboard";

    [ObservableProperty]
    private bool _isDarkMode;

    [ObservableProperty]
    private string _currentUserDisplay = "المستخدم النشط";

    [ObservableProperty]
    private bool _isCashier;

    [ObservableProperty]
    private bool _canAccessAdmin = true;

    [ObservableProperty]
    private bool _isSidebarCollapsed;

    // ── Screen Access Permissions ──
    public bool CanAccessDashboard => _authStateService.HasPermission(Permissions.Dashboard.View);
    public bool CanAccessPos => _authStateService.HasPermission(Permissions.Pos.Access);
    public bool CanAccessProducts => _authStateService.HasPermission(Permissions.Products.View);
    public bool CanAccessCategories => _authStateService.HasPermission(Permissions.Categories.View);
    public bool CanAccessUnits => _authStateService.HasPermission(Permissions.Units.View);
    public bool CanAccessCustomers => _authStateService.HasPermission(Permissions.Customers.View);
    public bool CanAccessSuppliers => _authStateService.HasPermission(Permissions.Suppliers.View);
    public bool CanAccessSalesInvoices => _authStateService.HasPermission(Permissions.SalesInvoices.View);
    public bool CanAccessSalesReturns => _authStateService.HasPermission(Permissions.SalesReturns.View);
    public bool CanAccessPurchaseInvoices => _authStateService.HasPermission(Permissions.PurchaseInvoices.View);
    public bool CanAccessPurchaseReturns => _authStateService.HasPermission(Permissions.PurchaseReturns.View);
    public bool CanAccessPurchaseOrders => _authStateService.HasPermission(Permissions.PurchaseOrders.View);
    public bool CanAccessWarehouses => _authStateService.HasPermission(Permissions.Warehouses.View);
    public bool CanAccessStock => _authStateService.HasPermission(Permissions.Stock.View);
    public bool CanAccessStockTransfers => _authStateService.HasPermission(Permissions.StockTransfers.View);
    public bool CanAccessStockAdjustments => _authStateService.HasPermission(Permissions.StockAdjustments.View);
    public bool CanAccessBranches => _authStateService.HasPermission(Permissions.Branches.View);
    public bool CanAccessUsers => _authStateService.HasPermission(Permissions.Users.View) || _authStateService.HasPermission(Permissions.Roles.View);
    public bool CanAccessTenants => _authStateService.HasPermission(Permissions.Tenants.View);

    // ── Group Visibility ──
    public bool CanAccessSalesGroup => CanAccessSalesInvoices || CanAccessSalesReturns || CanAccessCustomers;
    public bool CanAccessPurchaseGroup => CanAccessPurchaseInvoices || CanAccessPurchaseReturns || CanAccessPurchaseOrders || CanAccessSuppliers;
    public bool CanAccessStockGroup => CanAccessStock || CanAccessProducts || CanAccessCategories || CanAccessUnits || CanAccessWarehouses || CanAccessStockTransfers || CanAccessStockAdjustments;
    public bool CanAccessAdminGroup => CanAccessBranches || CanAccessUsers || CanAccessTenants;

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
    }

    public ShellViewModel(
        INavigationService navigationService, 
        AuthStateService authStateService,
        IThemeService themeService,
        IToastService toastService)
    {
        _navigationService = navigationService;
        _authStateService = authStateService;
        _themeService = themeService;
        ToastService = toastService;

        IsDarkMode = _themeService.IsDarkMode;
        _themeService.ThemeChanged += OnThemeChanged;
        _navigationService.Navigated += OnNavigated;
        _authStateService.AuthStateChanged += OnAuthStateChanged;

        RefreshPermissions();

        // التوجيه التلقائي المبدئي للشاشة المناسبة بحسب الصلاحيات
        if (IsCashier && CanAccessPos)
        {
            NavigateToPos();
        }
        else if (CanAccessDashboard)
        {
            NavigateToDashboard();
        }
        else if (CanAccessPos)
        {
            NavigateToPos();
        }
        else if (CanAccessProducts)
        {
            NavigateToProducts();
        }
        else if (CanAccessSalesInvoices)
        {
            NavigateToSalesInvoices();
        }
    }

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        RefreshPermissions();
    }

    public void RefreshPermissions()
    {
        IsCashier = _authStateService.IsCashier;
        CanAccessAdmin = _authStateService.IsAdmin;

        if (!string.IsNullOrEmpty(_authStateService.CurrentUserName))
        {
            CurrentUserDisplay = _authStateService.CurrentUserName;
        }

        OnPropertyChanged(nameof(CanAccessDashboard));
        OnPropertyChanged(nameof(CanAccessPos));
        OnPropertyChanged(nameof(CanAccessProducts));
        OnPropertyChanged(nameof(CanAccessCategories));
        OnPropertyChanged(nameof(CanAccessUnits));
        OnPropertyChanged(nameof(CanAccessCustomers));
        OnPropertyChanged(nameof(CanAccessSuppliers));
        OnPropertyChanged(nameof(CanAccessSalesInvoices));
        OnPropertyChanged(nameof(CanAccessSalesReturns));
        OnPropertyChanged(nameof(CanAccessPurchaseInvoices));
        OnPropertyChanged(nameof(CanAccessPurchaseReturns));
        OnPropertyChanged(nameof(CanAccessPurchaseOrders));
        OnPropertyChanged(nameof(CanAccessWarehouses));
        OnPropertyChanged(nameof(CanAccessStock));
        OnPropertyChanged(nameof(CanAccessStockTransfers));
        OnPropertyChanged(nameof(CanAccessStockAdjustments));
        OnPropertyChanged(nameof(CanAccessBranches));
        OnPropertyChanged(nameof(CanAccessUsers));
        OnPropertyChanged(nameof(CanAccessTenants));

        OnPropertyChanged(nameof(CanAccessSalesGroup));
        OnPropertyChanged(nameof(CanAccessPurchaseGroup));
        OnPropertyChanged(nameof(CanAccessStockGroup));
        OnPropertyChanged(nameof(CanAccessAdminGroup));
    }

    private void OnNavigated(object? sender, UserControl view)
    {
        CurrentView = view;
        ActiveSection = view.GetType().Name.Replace("View", "");
        CurrentTitle = ActiveSection switch
        {
            "Pos" => "نقطة البيع", "Dashboard" => "لوحة التحكم", "Products" => "المنتجات والأصناف",
            "Categories" => "التصنيفات", "Units" => "وحدات القياس", "Branches" => "الفروع",
            "Customers" => "العملاء", "Suppliers" => "الموردون", "Warehouses" => "المخازن وصالات العرض",
            "Stock" => "أرصدة المخزون", "StockTransfers" => "التحويلات المخزنية", "StockAdjustments" => "التسويات الجردية",
            "SalesInvoices" => "فواتير المبيعات", "SalesReturns" => "مرتجعات المبيعات",
            "PurchaseInvoices" => "فواتير المشتريات", "PurchaseOrders" => "أوامر الشراء", "PurchaseReturns" => "مرتجعات المشتريات",
            "Users" => "المستخدمون والصلاحيات", "Tenants" => "ملف المنشأة", _ => "مساحة العمل"
        };
    }

    private void OnThemeChanged(object? sender, AppTheme theme)
    {
        IsDarkMode = theme == AppTheme.Dark;
        OnPropertyChanged(nameof(ActiveSection));
        _navigationService.RefreshAppearance();
    }

    public void ReleaseNavigation()
    {
        _navigationService.Navigated -= OnNavigated;
        _themeService.ThemeChanged -= OnThemeChanged;
        _authStateService.AuthStateChanged -= OnAuthStateChanged;
    }

    public override void SaveWorkspace() => _navigationService.SaveWorkspace();

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        IsDarkMode = _themeService.IsDarkMode;
        ToastService.ShowInfo(IsDarkMode ? "تم تفعيل الوضع الداكن" : "تم تفعيل الوضع الفاتح", "المظهر");
    }

    [RelayCommand]
    private void NavigateToPos()
    {
        if (!CanAccessPos)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى نقطة البيع (POS)", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Pos";
        CurrentTitle = "نقطة البيع السريعة (POS)";
        _navigationService.NavigateTo<PosView>();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        if (!CanAccessDashboard)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى لوحة التحكم الإحصائية", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Dashboard";
        CurrentTitle = "لوحة التحكم الإحصائية";
        _navigationService.NavigateTo<DashboardView>();
    }

    [RelayCommand]
    private void NavigateToTenants()
    {
        if (!CanAccessTenants)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى ملف المنشأة", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Tenants";
        CurrentTitle = "الملف الشخصي للمستأجر";
        _navigationService.NavigateTo<TenantsView>();
    }

    [RelayCommand]
    private void NavigateToBranches()
    {
        if (!CanAccessBranches)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة الفروع", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Branches";
        CurrentTitle = "إدارة الفروع";
        _navigationService.NavigateTo<BranchesView>();
    }

    [RelayCommand]
    private void NavigateToCategories()
    {
        if (!CanAccessCategories)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى تصنيفات المنتجات", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Categories";
        CurrentTitle = "تصنيفات المنتجات";
        _navigationService.NavigateTo<CategoriesView>();
    }

    [RelayCommand]
    private void NavigateToProducts()
    {
        if (!CanAccessProducts)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة المنتجات والأصناف", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Products";
        CurrentTitle = "إدارة المنتجات والأصناف";
        _navigationService.NavigateTo<ProductsView>();
    }

    [RelayCommand]
    private void NavigateToUnits()
    {
        if (!CanAccessUnits)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى وحدات القياس", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Units";
        CurrentTitle = "وحدات القياس";
        _navigationService.NavigateTo<UnitsView>();
    }

    [RelayCommand]
    private void NavigateToCustomers()
    {
        if (!CanAccessCustomers)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة العملاء والحسابات", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Customers";
        CurrentTitle = "إدارة العملاء والحسابات";
        _navigationService.NavigateTo<CustomersView>();
    }

    [RelayCommand]
    private void NavigateToSalesInvoices()
    {
        if (!CanAccessSalesInvoices)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى فواتير المبيعات", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "SalesInvoices";
        CurrentTitle = "فواتير المبيعات";
        _navigationService.NavigateTo<SalesInvoicesView>();
    }

    [RelayCommand]
    private void NavigateToSalesReturns()
    {
        if (!CanAccessSalesReturns)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى مرتجعات المبيعات", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "SalesReturns";
        CurrentTitle = "مرتجعات المبيعات";
        _navigationService.NavigateTo<SalesReturnsView>();
    }

    [RelayCommand]
    private void NavigateToSuppliers()
    {
        if (!CanAccessSuppliers)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة الموردين", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Suppliers";
        CurrentTitle = "إدارة الموردين";
        _navigationService.NavigateTo<SuppliersView>();
    }

    [RelayCommand]
    private void NavigateToWarehouses()
    {
        if (!CanAccessWarehouses)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى المخازن وصالات العرض", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Warehouses";
        CurrentTitle = "المخازن وصالات العرض";
        _navigationService.NavigateTo<WarehousesView>();
    }

    [RelayCommand]
    private void NavigateToStock()
    {
        if (!CanAccessStock)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة المخزون والأرصدة", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Stock";
        CurrentTitle = "إدارة المخزون والأرصدة";
        _navigationService.NavigateTo<StockView>();
    }

    [RelayCommand]
    private void NavigateToStockTransfers()
    {
        if (!CanAccessStockTransfers)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى التحويلات المخزنية", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "StockTransfers";
        CurrentTitle = "التحويلات المخزنية";
        _navigationService.NavigateTo<StockTransfersView>();
    }

    [RelayCommand]
    private void NavigateToStockAdjustments()
    {
        if (!CanAccessStockAdjustments)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى التسويات الجردية", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "StockAdjustments";
        CurrentTitle = "التسويات الجردية";
        _navigationService.NavigateTo<StockAdjustmentsView>();
    }

    [RelayCommand]
    private void NavigateToPurchaseOrders()
    {
        if (!CanAccessPurchaseOrders)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى طلبات وأوامر الشراء", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "PurchaseOrders";
        CurrentTitle = "الطلبيات وأوامر الشراء";
        _navigationService.NavigateTo<PurchaseOrdersView>();
    }

    [RelayCommand]
    private void NavigateToPurchaseInvoices()
    {
        if (!CanAccessPurchaseInvoices)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى فواتير المشتريات المباشرة", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "PurchaseInvoices";
        CurrentTitle = "فواتير المشتريات المباشرة";
        _navigationService.NavigateTo<PurchaseInvoicesView>();
    }

    [RelayCommand]
    private void NavigateToPurchaseReturns()
    {
        if (!CanAccessPurchaseReturns)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى مرتجعات المشتريات للموردين", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "PurchaseReturns";
        CurrentTitle = "مرتجعات المشتريات للموردين";
        _navigationService.NavigateTo<PurchaseReturnsView>();
    }

    [RelayCommand]
    private void NavigateToUsers()
    {
        if (!CanAccessUsers)
        {
            ToastService.ShowWarning("ليس لديك صلاحية الوصول إلى إدارة المستخدمين والصلاحيات", "صلاحية مرفوضة");
            return;
        }
        ActiveSection = "Users";
        CurrentTitle = "إدارة المستخدمين والصلاحيات";
        _navigationService.NavigateTo<RetalSystemAPI.Desktop.Views.Users.UsersView>();
    }

    [RelayCommand]
    private void Logout()
    {
        _navigationService.SaveWorkspace();
        _navigationService.ResetSession();
        _authStateService.ClearToken();
    }
}
