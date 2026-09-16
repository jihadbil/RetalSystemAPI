using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Common;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Models.Dashboard;
using RetalSystemAPI.Desktop.Models.Pos;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.Services.Sales;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Dashboard;
using RetalSystemAPI.Desktop.ViewModels.Pos;
using RetalSystemAPI.Desktop.Views.Catalog;
using RetalSystemAPI.Desktop.Views.Pos;

internal static class Program
{
    private static int _passed;
    private static readonly BranchDto Branch = new() { Id = Guid.NewGuid(), Name = "الفرع الرئيسي" };
    private static readonly WarehouseSummaryDto Warehouse = new() { Id = Guid.NewGuid(), BranchId = Branch.Id, Name = "صالة العرض", TypeName = "عرض" };
    private static readonly CustomerSummaryDto Customer = new() { Id = Guid.NewGuid(), Name = "عميل تجريبي" };
    private static readonly List<ProductDto> Products = Enumerable.Range(1, 27).Select(i => new ProductDto
    {
        Id = Guid.NewGuid(), Name = i % 2 == 0 ? $"قهوة عربية محمصة {i}" : $"عبوة مياه معدنية {i}", Code = $"P{i:000}",
        CategoryName = "المشروبات", SalePrice = 100, CostPrice = 70
    }).ToList();

    [STAThread]
    public static int Main(string[] args)
    {
        var app = new RetalSystemAPI.Desktop.App();
        app.InitializeComponent(); // No startup: no credentials, real APIs, or store data.
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        var result = 0;
        var task = RunAsync(args);
        var frame = new DispatcherFrame();
        task.ContinueWith(_ => app.Dispatcher.BeginInvoke(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        try { task.GetAwaiter().GetResult(); Console.WriteLine($"PASS: {_passed} checks"); }
        catch (Exception ex) { Console.Error.WriteLine(ex); result = 1; }
        app.Shutdown();
        return result;
    }

    private static Task<ApiResponse<T>> Ok<T>(T data) => Task.FromResult(new ApiResponse<T> { Success = true, Data = data });
    private static IBranchApiService BranchApi() => Stub.Create<IBranchApiService>((_, _) => Ok(new List<BranchDto> { Branch }));
    private static ICustomerApiService CustomerApi() => Stub.Create<ICustomerApiService>((method, _) => method == "GetPagedAsync"
        ? Ok(new PagedResult<CustomerSummaryDto> { Items = new() { Customer }, TotalCount = 1, TotalPages = 1 }) : Ok(new List<CustomerSummaryDto> { Customer }));
    private static IWarehouseApiService WarehouseApi() => Stub.Create<IWarehouseApiService>((_, _) => Ok(new List<WarehouseSummaryDto> { Warehouse }));
    private static IProductApiService ProductApi() => Stub.Create<IProductApiService>((method, args) => method switch
    {
        "GetAllAsync" or "SearchAsync" => Ok(Products),
        "GetPagedAsync" => Ok(new PagedResult<ProductDto> { Items = Products.Skip(((int)args[0]! - 1) * (int)args[1]!).Take((int)args[1]!).ToList(), TotalCount = Products.Count, TotalPages = (int)Math.Ceiling((double)Products.Count / (int)args[1]!) }),
        _ => Ok(Products[0])
    });

    private static PosViewModel Pos(FakeSession session, Action<CreateSalesInvoiceRequest>? onSave = null, bool failSave = false, IProductApiService? productApi = null) => new(
        BranchApi(), WarehouseApi(), CustomerApi(), Stub.Create<ICategoryApiService>((_, _) => Ok(new List<CategoryDto>())), productApi ?? ProductApi(),
        Stub.Create<IStockApiService>((_, _) => Ok(new List<ShowroomStockDto>())),
        Stub.Create<ISalesInvoiceApiService>((_, args) =>
        {
            onSave?.Invoke((CreateSalesInvoiceRequest)args[0]!);
            return Task.FromResult(new ApiResponse<SalesInvoiceDto> { Success = !failSave, Data = failSave ? null : new SalesInvoiceDto(), Message = "فشل تجريبي" });
        }), new FakeToast(), session);

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException("FAIL: " + name);
        _passed++; Console.WriteLine("PASS: " + name);
    }

    private static async Task RunAsync(string[] args)
    {
        var session = new FakeSession();
        CreateSalesInvoiceRequest? submitted = null;
        var vm = Pos(session, request => submitted = request);
        vm.AddProductToCart(Products[0]);
        vm.PaidAmount = 50;
        Check(vm.PaidAmount == 50 && vm.PaymentShortfall == 50, "Underpayment remains entered; shortfall visible");
        await vm.SubmitSaleCommand.ExecuteAsync(null);
        Check(submitted == null && vm.CartItems.Count == 1 && vm.ErrorMessage != null, "Underpaid cash never submitted");
        vm.ClearCartCommand.Execute(null);
        Check(vm.CartItems.Count == 1, "Clearing requires explicit confirmation");
        vm.SelectedCustomer = Customer; vm.OrderDiscount = 10; vm.TaxAmount = 5; vm.PaidAmount = 20;
        vm.OrderNotes = "ملاحظات محفوظة"; vm.SelectedPaymentMethod = PaymentMethod.Credit;
        vm.HoldOrderCommand.Execute(null);
        Check(vm.HeldOrders.Count == 1 && vm.CartItems.Count == 0 && vm.PaidAmount == 0 && vm.OrderDiscount == 0 && vm.SelectedCustomer == null, "Hold resets active payment and customer");
        vm.AddProductToCart(Products[1]);
        await vm.RestoreHeldOrderCommand.ExecuteAsync(null);
        Check(vm.CartItems.Single().ProductId == Products[1].Id && vm.HeldOrders.Count == 1, "Restore protects active cart");
        vm.HoldOrderCommand.Execute(null);
        Check(vm.HeldOrders.Count == 2, "Multiple held orders retained");
        vm.SelectedHeldOrder = vm.HeldOrders[0];
        await vm.RestoreHeldOrderCommand.ExecuteAsync(null);
        Check(vm.SelectedCustomer?.Id == Customer.Id && vm.OrderDiscount == 10 && vm.TaxAmount == 5 && vm.PaidAmount == 20 && vm.OrderNotes == "ملاحظات محفوظة" && vm.SelectedPaymentMethod == PaymentMethod.Credit, "Restore preserves full sale context");
        vm.SaveWorkspace();
        var restored = Pos(session);
        Check(restored.CartItems.Count == 1 && restored.HeldOrders.Count == 1 && restored.OrderNotes == vm.OrderNotes, "Fresh view model restores active and held drafts");
        restored.CartItems[0].Quantity = 3;
        Check(vm.CartItems[0].Quantity == 1, "Saved snapshots do not share mutable cart items");
        vm.SelectedPaymentMethod = PaymentMethod.Card;
        await vm.SubmitSaleCommand.ExecuteAsync(null);
        Check(submitted?.PaidAmount == 95 && vm.CartItems.Count == 0 && session.State.Active == null, "Card settles exact total and removes completed draft");
        var retryNumbers = new List<string>();
        var failed = Pos(new FakeSession(), request => retryNumbers.Add(request.InvoiceNumber), failSave: true);
        failed.AddProductToCart(Products[0]); failed.SetQuickCashCommand.Execute("0");
        await failed.SubmitSaleCommand.ExecuteAsync(null);
        Check(failed.CartItems.Count == 1 && failed.ErrorMessage != null, "Save failure retains cart");
        await failed.SubmitSaleCommand.ExecuteAsync(null);
        Check(retryNumbers.Distinct().Count() == 1, "Retry reuses invoice number to prevent duplicate sales");
        var products = new ProductsViewModel(ProductApi());
        products.CurrentPage = 3; products.SearchQuery = "P";
        await products.SearchCommand.ExecuteAsync(null);
        Check(products.CurrentPage == 1 && products.TotalPages == 3 && products.Products.Count == 10, "Search resets page and paginates results");
        await products.NextPageCommand.ExecuteAsync(null);
        Check(products.CurrentPage == 2 && products.Products[0].Id == Products[10].Id, "Search next page changes rows");
        var broken = new ProductsViewModel(Stub.Create<IProductApiService>((_, _) => Task.FromResult(new ApiResponse<PagedResult<ProductDto>> { Success = false })));
        Check(broken.ErrorMessage != null && !broken.IsLoading, "Product API failure is explicit");
        var toast = new FakeToast();
        var dashboard = new DashboardViewModel(Stub.Create<IDashboardApiService>((_, _) => Task.FromResult(new ApiResponse<DashboardSummaryDto> { Success = false })), BranchApi(), Stub.Create<INavigationService>((_, _) => null), toast);
        await dashboard.RefreshCommand.ExecuteAsync(null);
        Check(toast.SuccessCount == 0, "Failed dashboard refresh never reports success");

        var delayed = new TaskCompletionSource<ApiResponse<List<ProductDto>>>();
        var raceApi = Stub.Create<IProductApiService>((method, request) => method == "SearchAsync"
            ? (string)request[0]! == "old" ? delayed.Task : Ok(new List<ProductDto> { Products[1] })
            : Ok(new PagedResult<ProductDto> { Items = Products.Take(10).ToList(), TotalPages = 3, TotalCount = 27 }));
        var racing = new ProductsViewModel(raceApi) { SearchQuery = "old" };
        var first = racing.LoadProductsAsync();
        racing.SearchQuery = "new";
        await racing.LoadProductsAsync();
        delayed.SetResult(new ApiResponse<List<ProductDto>> { Success = true, Data = new() { Products[0] } });
        await first;
        Check(racing.Products.Single().Id == Products[1].Id, "Older response cannot replace latest search results");

        var fakeAuth = (RetalSystemAPI.Desktop.Core.Auth.AuthStateService)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(RetalSystemAPI.Desktop.Core.Auth.AuthStateService));
        void SetAuth(string user)
        {
            typeof(RetalSystemAPI.Desktop.Core.Auth.AuthStateService).GetProperty("CurrentUserId")!.SetValue(fakeAuth, user);
            typeof(RetalSystemAPI.Desktop.Core.Auth.AuthStateService).GetProperty("JwtToken")!.SetValue(fakeAuth, "test-only-not-a-real-token");
            typeof(RetalSystemAPI.Desktop.Core.Auth.AuthStateService).GetProperty("CurrentTenantId")!.SetValue(fakeAuth, Branch.Id);
        }
        SetAuth("test-user-a");
        var storage = Path.Combine(Path.GetTempPath(), "retal-ux-" + Guid.NewGuid().ToString("N"));
        try
        {
            var disk = new PosSessionService(fakeAuth, storage);
            var scopeA = disk.ScopeKey!;
            disk.Save(scopeA, new PosSessionState { Active = new PosDraft { Notes = "saved across restart" } });
            Check(new PosSessionService(fakeAuth, storage).Load().Active?.Notes == "saved across restart", "Draft survives a new disk service instance");
            SetAuth("test-user-b");
            Check(disk.Load().Active == null, "Other signed-in user cannot load prior drafts");
            disk.Save(scopeA, new PosSessionState { Active = new PosDraft { Notes = "stale writer" } });
            Check(disk.Load().Active == null && Directory.GetFiles(storage).Length == 1, "Old view cannot write to next user's session");
        }
        finally
        {
            if (Directory.Exists(storage))
            {
                foreach (var file in Directory.GetFiles(storage)) File.Delete(file);
                Directory.Delete(storage);
            }
        }

        var productForm = new ProductFormViewModel(ProductApi(), Stub.Create<ICategoryApiService>((_, _) => Ok(new List<CategoryDto>())),
            Stub.Create<IUnitApiService>((_, _) => Ok(new List<UnitDto>())), Stub.Create<IProductImageApiService>((_, _) => null), WarehouseApi());
        await productForm.InitializeAsync(null);
        productForm.CostPrice = -1;
        Check(productForm.GetErrors(nameof(ProductFormViewModel.CostPrice)).Any(), "Negative product cost has field-level validation");
        productForm.CostPrice = 70;
        Check(!productForm.GetErrors(nameof(ProductFormViewModel.CostPrice)).Any(), "Correcting field clears its validation error");

        var provider = new ServiceCollection().AddSingleton(fakeAuth).AddSingleton<IProductApiService>(ProductApi())
            .AddTransient<ProductsViewModel>().AddTransient<ProductsView>().AddSingleton<INavigationService, NavigationService>().BuildServiceProvider();
        var navigation = provider.GetRequiredService<INavigationService>();
        var shell = new RetalSystemAPI.Desktop.ViewModels.Shell.ShellViewModel(navigation, fakeAuth,
            Stub.Create<IThemeService>((name, _) => name == "get_IsDarkMode" ? false : null), new FakeToast());
        navigation.NavigateTo<ProductsView>();
        var originalView = shell.CurrentView;
        navigation.NavigateTo(new UserControl());
        navigation.NavigateTo<ProductsView>();
        Check(ReferenceEquals(originalView, shell.CurrentView) && shell.ActiveSection == "Products" && shell.CurrentTitle == "المنتجات والأصناف", "Navigation preserves view state and synchronizes title and active section");
        navigation.ResetSession();
        navigation.NavigateTo<ProductsView>();
        Check(!ReferenceEquals(originalView, shell.CurrentView), "Session reset releases cached views");
        shell.ReleaseNavigation();

        var scanner = Pos(new FakeSession());
        scanner.BarcodeQuery = "P001";
        var scan1 = scanner.AddBarcodeCommand.ExecuteAsync(null);
        scanner.BarcodeQuery = "P002";
        var scan2 = scanner.AddBarcodeCommand.ExecuteAsync(null);
        await Task.WhenAll(scan1, scan2);
        Check(scanner.CartItems.Count == 2 && scanner.BarcodeQuery.Length == 0, "Sequential scanner input adds both products");
        var lookup = new TaskCompletionSource<ApiResponse<ProductDto>>();
        var delayedBarcodeApi = Stub.Create<IProductApiService>((method, _) => method == "GetAllAsync" ? Ok(new List<ProductDto>()) : lookup.Task);
        var queuedScanner = Pos(new FakeSession(), productApi: delayedBarcodeApi);
        queuedScanner.BarcodeQuery = "P001";
        var queuedFirst = queuedScanner.AddBarcodeCommand.ExecuteAsync(null);
        queuedScanner.BarcodeQuery = "P001";
        await queuedScanner.AddBarcodeCommand.ExecuteAsync(null);
        await queuedScanner.SubmitSaleCommand.ExecuteAsync(null);
        Check(queuedScanner.IsScanning && queuedScanner.ErrorMessage?.Contains("انتظر") == true, "Checkout waits for pending barcode lookups");
        lookup.SetResult(new ApiResponse<ProductDto> { Success = true, Data = Products[0] });
        await queuedFirst;
        Check(queuedScanner.CartItems.Single().Quantity == 2 && !queuedScanner.IsScanning, "Slow barcode lookup does not lose the next scan");

        if (args.Contains("--render"))
        {
            var output = Path.GetFullPath(Path.Combine("artifacts", "desktop-ux"));
            Directory.CreateDirectory(output);
            var preview = Pos(new FakeSession());
            foreach (var product in Products.Take(4)) preview.AddProductToCart(product);
            preview.PaidAmount = 500;
            foreach (var theme in new[] { "Light", "Dark" })
            {
                Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary { Source = new Uri($"/RetalSystemAPI.Desktop;component/Resources/Themes/{theme}Theme.xaml", UriKind.Relative) };
                await RenderAsync(new PosView(preview, CustomerApi()), 1093, 614, Path.Combine(output, $"pos-{theme}.png"));
                await RenderAsync(new ProductsView(products), 1093, 614, Path.Combine(output, $"products-{theme}.png"));
                var customers = new RetalSystemAPI.Desktop.ViewModels.Customers.CustomersViewModel(CustomerApi());
                await RenderAsync(new RetalSystemAPI.Desktop.Views.Customers.CustomersView(customers), 1093, 614, Path.Combine(output, $"customers-{theme}.png"));
                var form = new RetalSystemAPI.Desktop.Views.Catalog.Dialogs.ProductFormDialog(productForm);
                var formContent = (FrameworkElement)form.Content;
                form.Content = null; formContent.DataContext = productForm; formContent.FlowDirection = FlowDirection.RightToLeft;
                await RenderAsync(formContent, 880, 700, Path.Combine(output, $"product-form-{theme}.png"));
            }
            await RenderAsync(new PosView(preview, CustomerApi()), 680, 520, Path.Combine(output, "pos-compact.png"));
            Check(true, "Rendered production XAML in both themes and compact layout");
        }
    }

    private static async Task RenderAsync(FrameworkElement view, int width, int height, string path)
    {
        var root = new System.Windows.Documents.AdornerDecorator { Child = view, Width = width, Height = height };
        using var source = new System.Windows.Interop.HwndSource(new System.Windows.Interop.HwndSourceParameters("UX test render")
        {
            Width = width, Height = height, WindowStyle = unchecked((int)0x80000000), PositionX = -20000, PositionY = -20000
        });
        source.SizeToContent = SizeToContent.WidthAndHeight;
        source.RootVisual = root;
        root.Measure(new Size(width, height)); root.Arrange(new Rect(0, 0, width, height)); root.UpdateLayout();
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        root.UpdateLayout();
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(root);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); encoder.Save(stream);
        Console.WriteLine("RENDER: " + path);
    }
}

public class Stub : DispatchProxy
{
    public Func<string, object?[], object?> Handler { get; set; } = null!;
    public static T Create<T>(Func<string, object?[], object?> handler) where T : class
    {
        var proxy = Create<T, Stub>(); ((Stub)(object)proxy).Handler = handler; return proxy;
    }
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!.Name, args ?? Array.Empty<object?>());
}

internal sealed class FakeSession : IPosSessionService
{
    public string? ScopeKey => "test-tenant:test-user";
    public PosSessionState State { get; private set; } = new();
    public PosSessionState Load() => State.Copy();
    public void Save(string scopeKey, PosSessionState state) => State = state.Copy();
}

internal sealed class FakeToast : IToastService
{
    public int SuccessCount { get; private set; }
    public ObservableCollection<ToastItem> ActiveToasts { get; } = new();
    public void Show(string title, string message, ToastType type = ToastType.Info, int durationSeconds = 3) { }
    public void ShowSuccess(string message, string title = "") => SuccessCount++;
    public void ShowError(string message, string title = "") { }
    public void ShowWarning(string message, string title = "") { }
    public void ShowInfo(string message, string title = "") { }
    public void Dismiss(ToastItem toast) { }
}
