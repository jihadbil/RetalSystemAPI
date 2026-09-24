using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Accounting;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.Services.Purchase;
using RetalSystemAPI.Desktop.Services.Sales;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Auth;
using RetalSystemAPI.Desktop.ViewModels.Branches;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Customers;
using RetalSystemAPI.Desktop.ViewModels.Dashboard;
using RetalSystemAPI.Desktop.ViewModels.Pos;
using RetalSystemAPI.Desktop.ViewModels.Purchase;
using RetalSystemAPI.Desktop.ViewModels.Sales;
using RetalSystemAPI.Desktop.ViewModels.Shell;
using RetalSystemAPI.Desktop.ViewModels.Stock;
using RetalSystemAPI.Desktop.ViewModels.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Tenants;
using RetalSystemAPI.Desktop.ViewModels.Warehouses;
using RetalSystemAPI.Desktop.Views.Accounting;
using RetalSystemAPI.Desktop.Views.Auth;
using RetalSystemAPI.Desktop.Views.Branches;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Customers;
using RetalSystemAPI.Desktop.Views.Dashboard;
using RetalSystemAPI.Desktop.Views.Pos;
using RetalSystemAPI.Desktop.Views.Purchase;
using RetalSystemAPI.Desktop.Views.Sales;
using RetalSystemAPI.Desktop.Views.Shell;
using RetalSystemAPI.Desktop.Views.Stock;
using RetalSystemAPI.Desktop.Views.Suppliers;
using RetalSystemAPI.Desktop.Views.Tenants;
using RetalSystemAPI.Desktop.Views.Warehouses;

namespace RetalSystemAPI.Desktop;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;
    private Window? _currentWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Helpers.WorkspaceWindowBehavior.Register();

        // منع إغلاق التطبيق تلقائياً عند إغلاق أو انتقال النوافذ
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        var authState = Services.GetRequiredService<AuthStateService>();

        // الاستماع لتغيرات المصادقة للتوجيه تلقائياً إلى تسجيل الدخول عند انتهاء الجلسة
        authState.AuthStateChanged += (s, e) =>
        {
            if (!authState.IsAuthenticated)
            {
                Dispatcher.Invoke(() => ShowLoginWindow());
            }
        };

        if (authState.IsAuthenticated)
        {
            ShowMainWindow();
        }
        else
        {
            ShowLoginWindow();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuredApiUrl = Environment.GetEnvironmentVariable("RETAL_API_BASE_URL")
            ?? "https://localhost:7226/api/";
        if (!Uri.TryCreate(configuredApiUrl, UriKind.Absolute, out var apiBaseUri) ||
            apiBaseUri.Scheme != Uri.UriSchemeHttps ||
            !apiBaseUri.AbsolutePath.EndsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "RETAL_API_BASE_URL must be an absolute HTTPS URL ending with /api/.");
        }

        var apiClientBuilder = services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = apiBaseUri;
            client.Timeout = TimeSpan.FromSeconds(30);
        });

#if DEBUG
        // Development certificates are accepted only by Debug builds and only
        // when connecting to localhost. Release builds always use normal TLS
        // certificate validation.
        if (apiBaseUri.IsLoopback)
        {
            apiClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        }
#endif

        // Core Services
        services.AddSingleton<CredentialStoreService>();
        services.AddSingleton<AuthStateService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IToastService, ToastService>();
        services.AddSingleton<IPosSessionService, PosSessionService>();

        // API Services
        services.AddTransient<IAuthApiService, AuthApiService>();
        services.AddTransient<ITenantApiService, TenantApiService>();
        services.AddTransient<IBranchApiService, BranchApiService>();
        services.AddTransient<ICategoryApiService, CategoryApiService>();
        services.AddTransient<IProductApiService, ProductApiService>();
        services.AddTransient<IUnitApiService, UnitApiService>();
        services.AddTransient<IProductUnitApiService, ProductUnitApiService>();
        services.AddTransient<IProductBarCodeApiService, ProductBarCodeApiService>();
        services.AddTransient<IProductImageApiService, ProductImageApiService>();

        services.AddTransient<ICustomerApiService, CustomerApiService>();
        services.AddTransient<ISalesInvoiceApiService, SalesInvoiceApiService>();
        services.AddTransient<ISalesReturnApiService, SalesReturnApiService>();

        services.AddTransient<ISupplierApiService, SupplierApiService>();
        services.AddTransient<IWarehouseApiService, WarehouseApiService>();
        services.AddTransient<IStockApiService, StockApiService>();
        services.AddTransient<IStockTransferApiService, StockTransferApiService>();
        services.AddTransient<IStockAdjustmentApiService, StockAdjustmentApiService>();
        services.AddTransient<IPurchaseOrderApiService, PurchaseOrderApiService>();
        services.AddTransient<IPurchaseInvoiceApiService, PurchaseInvoiceApiService>();
        services.AddTransient<IPurchaseReturnApiService, PurchaseReturnApiService>();
        services.AddTransient<IDashboardApiService, DashboardApiService>();
        services.AddTransient<RetalSystemAPI.Desktop.Services.Users.IUserApiService, RetalSystemAPI.Desktop.Services.Users.UserApiService>();
        services.AddTransient<ISafeApiService, SafeApiService>();
        services.AddTransient<IExpenseCategoryApiService, ExpenseCategoryApiService>();
        services.AddTransient<IExpenseApiService, ExpenseApiService>();
        services.AddTransient<IPosCashApiService, PosCashApiService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<PosViewModel>();
        services.AddTransient<TenantsViewModel>();
        services.AddTransient<BranchesViewModel>();
        services.AddTransient<BranchFormViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<CategoryFormViewModel>();
        services.AddTransient<UnitsViewModel>();
        services.AddTransient<UnitFormViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ProductFormViewModel>();
        services.AddTransient<ProductImportWizardViewModel>();

        services.AddTransient<CustomersViewModel>();
        services.AddTransient<CustomerFormViewModel>();
        services.AddTransient<SalesInvoicesViewModel>();
        services.AddTransient<SalesInvoiceFormViewModel>();
        services.AddTransient<SalesReturnsViewModel>();
        services.AddTransient<SalesReturnFormViewModel>();

        services.AddTransient<SuppliersViewModel>();
        services.AddTransient<SupplierFormViewModel>();
        services.AddTransient<WarehousesViewModel>();
        services.AddTransient<WarehouseFormViewModel>();
        services.AddTransient<StockViewModel>();
        services.AddTransient<SetStockFormViewModel>();
        services.AddTransient<StockTransfersViewModel>();
        services.AddTransient<StockTransferFormViewModel>();
        services.AddTransient<StockAdjustmentsViewModel>();
        services.AddTransient<StockAdjustmentFormViewModel>();
        services.AddTransient<PurchaseOrdersViewModel>();
        services.AddTransient<PurchaseOrderFormViewModel>();
        services.AddTransient<PurchaseInvoicesViewModel>();
        services.AddTransient<PurchaseInvoiceFormViewModel>();
        services.AddTransient<PurchaseReturnsViewModel>();
        services.AddTransient<PurchaseReturnFormViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.UsersViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.UserFormViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.ResetPasswordViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.RolePermissionsViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.UserPermissionsViewModel>();
        services.AddTransient<SafesViewModel>();
        services.AddTransient<SafeFormViewModel>();
        services.AddTransient<SafeTransferViewModel>();
        services.AddTransient<SafeTransactionFormViewModel>();
        services.AddTransient<SafeLedgerViewModel>();
        services.AddTransient<ExpenseCategoriesViewModel>();
        services.AddTransient<ExpenseCategoryFormViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<ExpenseFormViewModel>();
        services.AddTransient<CustomerStatementViewModel>();
        services.AddTransient<CustomerPaymentViewModel>();
        services.AddTransient<SupplierStatementViewModel>();
        services.AddTransient<SupplierPaymentViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<ShellWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<PosView>();
        services.AddTransient<TenantsView>();
        services.AddTransient<BranchesView>();
        services.AddTransient<CategoriesView>();
        services.AddTransient<UnitsView>();
        services.AddTransient<ProductsView>();

        services.AddTransient<CustomersView>();
        services.AddTransient<SalesInvoicesView>();
        services.AddTransient<SalesReturnsView>();

        services.AddTransient<SuppliersView>();
        services.AddTransient<WarehousesView>();
        services.AddTransient<StockView>();
        services.AddTransient<StockTransfersView>();
        services.AddTransient<StockAdjustmentsView>();
        services.AddTransient<PurchaseOrdersView>();
        services.AddTransient<PurchaseInvoicesView>();
        services.AddTransient<PurchaseReturnsView>();
        services.AddTransient<SafesView>();
        services.AddTransient<ExpenseCategoriesView>();
        services.AddTransient<ExpensesView>();
        services.AddTransient<CustomerStatementWindow>();
        services.AddTransient<CustomerPaymentWindow>();
        services.AddTransient<SupplierStatementWindow>();
        services.AddTransient<SupplierPaymentWindow>();
        services.AddTransient<RetalSystemAPI.Desktop.Views.Users.UsersView>();
        services.AddTransient<RetalSystemAPI.Desktop.Views.Users.RolePermissionsWindow>();
        services.AddTransient<RetalSystemAPI.Desktop.Views.Users.UserPermissionsWindow>();
    }

    public void ShowLoginWindow()
    {
        Services.GetRequiredService<INavigationService>().ResetSession();
        var oldWindow = _currentWindow;

        var loginView = Services.GetRequiredService<LoginView>();
        var loginVM = (LoginViewModel)loginView.DataContext;
        loginVM.OnLoginSuccess = () => ShowMainWindow();

        var loginWindow = new Window
        {
            Title = "تسجيل الدخول - Emerald Management Pro",
            Content = loginView,
            Width = 460,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        loginWindow.Closed += (s, e) =>
        {
            if (_currentWindow == loginWindow)
            {
                Shutdown();
            }
        };

        _currentWindow = loginWindow;
        MainWindow = loginWindow;
        loginWindow.Show();
        oldWindow?.Close();
    }

    public void ShowMainWindow()
    {
        var oldWindow = _currentWindow;

        var shellWindow = Services.GetRequiredService<ShellWindow>();
        _currentWindow = shellWindow;
        MainWindow = shellWindow;

        shellWindow.Closed += (s, e) =>
        {
            ((ShellViewModel)shellWindow.DataContext).ReleaseNavigation();
            if (_currentWindow == shellWindow)
            {
                Shutdown();
            }
        };

        shellWindow.Show();
        oldWindow?.Close();

        // Navigate to Dashboard initially
        var nav = Services.GetRequiredService<INavigationService>();
        nav.NavigateTo<DashboardView>();
    }
}
