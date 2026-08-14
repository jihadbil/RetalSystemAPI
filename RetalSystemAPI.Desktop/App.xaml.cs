using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Purchase;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Auth;
using RetalSystemAPI.Desktop.ViewModels.Branches;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Dashboard;
using RetalSystemAPI.Desktop.ViewModels.Purchase;
using RetalSystemAPI.Desktop.ViewModels.Shell;
using RetalSystemAPI.Desktop.ViewModels.Stock;
using RetalSystemAPI.Desktop.ViewModels.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Tenants;
using RetalSystemAPI.Desktop.ViewModels.Warehouses;
using RetalSystemAPI.Desktop.Views.Auth;
using RetalSystemAPI.Desktop.Views.Branches;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Dashboard;
using RetalSystemAPI.Desktop.Views.Purchase;
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
        // Base API Address with Local SSL Bypass
        services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://localhost:7226/api/");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        // Core Services
        services.AddSingleton<CredentialStoreService>();
        services.AddSingleton<AuthStateService>();
        services.AddSingleton<INavigationService, NavigationService>();

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

        services.AddTransient<ISupplierApiService, SupplierApiService>();
        services.AddTransient<IWarehouseApiService, WarehouseApiService>();
        services.AddTransient<IStockApiService, StockApiService>();
        services.AddTransient<IPurchaseOrderApiService, PurchaseOrderApiService>();
        services.AddTransient<RetalSystemAPI.Desktop.Services.Users.IUserApiService, RetalSystemAPI.Desktop.Services.Users.UserApiService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<TenantsViewModel>();
        services.AddTransient<BranchesViewModel>();
        services.AddTransient<BranchFormViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<CategoryFormViewModel>();
        services.AddTransient<UnitsViewModel>();
        services.AddTransient<UnitFormViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<ProductFormViewModel>();

        services.AddTransient<SuppliersViewModel>();
        services.AddTransient<SupplierFormViewModel>();
        services.AddTransient<WarehousesViewModel>();
        services.AddTransient<WarehouseFormViewModel>();
        services.AddTransient<StockViewModel>();
        services.AddTransient<SetStockFormViewModel>();
        services.AddTransient<PurchaseOrdersViewModel>();
        services.AddTransient<PurchaseOrderFormViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.UsersViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.UserFormViewModel>();
        services.AddTransient<RetalSystemAPI.Desktop.ViewModels.Users.ResetPasswordViewModel>();

        // Views
        services.AddTransient<LoginView>();
        services.AddTransient<ShellWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<TenantsView>();
        services.AddTransient<BranchesView>();
        services.AddTransient<CategoriesView>();
        services.AddTransient<UnitsView>();
        services.AddTransient<ProductsView>();

        services.AddTransient<SuppliersView>();
        services.AddTransient<WarehousesView>();
        services.AddTransient<StockView>();
        services.AddTransient<PurchaseOrdersView>();
        services.AddTransient<RetalSystemAPI.Desktop.Views.Users.UsersView>();
    }

    public void ShowLoginWindow()
    {
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
        loginWindow.Show();
        oldWindow?.Close();
    }

    public void ShowMainWindow()
    {
        var oldWindow = _currentWindow;

        var shellWindow = Services.GetRequiredService<ShellWindow>();
        _currentWindow = shellWindow;

        shellWindow.Closed += (s, e) =>
        {
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
