using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Models.Pos;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.Services.Sales;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Pos;

public partial class PosViewModel : BaseViewModel
{
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly ICustomerApiService _customerApiService;
    private readonly ICategoryApiService _categoryApiService;
    private readonly IProductApiService _productApiService;
    private readonly IStockApiService _stockApiService;
    private readonly ISalesInvoiceApiService _salesInvoiceApiService;
    private readonly IToastService _toastService;
    private readonly IPosSessionService _sessionService;
    private readonly string? _sessionScope;
    private readonly System.Windows.Threading.DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private bool _restoring = true;
    private bool _draftReadFailed;
    private Guid _activeDraftId = Guid.NewGuid();
    [ObservableProperty] private string? _draftStatus;
    [ObservableProperty] private decimal _paymentShortfall;
    [ObservableProperty] private ObservableCollection<PosDraft> _heldOrders = new();
    [ObservableProperty] private PosDraft? _selectedHeldOrder;
    public Func<string, string, bool>? ConfirmActionHandler { get; set; }

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        _ = LoadShowroomStocksAsync();
    }

    [ObservableProperty]
    private ObservableCollection<CustomerSummaryDto> _customers = new();

    [ObservableProperty]
    private CustomerSummaryDto? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<CategoryFilterItem> _categoryTabs = new();

    [ObservableProperty]
    private CategoryFilterItem? _selectedCategoryTab;

    private List<ProductDto> _allProducts = new();
    private List<CategoryDto> _allCategories = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> _cartItems = new();

    [ObservableProperty]
    private ObservableCollection<QuickProductItem> _quickItems = new();

    [ObservableProperty]
    private bool _isQuickItemsVisible;

    [ObservableProperty]
    private string _barcodeQuery = string.Empty;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _orderDiscount;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _paidAmount;

    [ObservableProperty]
    private decimal _changeAmount;

    [ObservableProperty]
    private decimal _customerDebtAmount;

    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private string _orderNotes = string.Empty;



    [ObservableProperty]
    private bool _hasHeldOrder;

    [ObservableProperty]
    private int _totalItemCount;


    public event Action? RequestBarcodeFocus;
    public Func<List<ProductDto>, List<CategoryDto>, Task<ProductDto?>>? ShowProductSearchDialogHandler { get; set; }
    public Func<Task<CustomerSummaryDto?>>? ShowQuickCustomerDialogHandler { get; set; }
    public Func<CartItemModel, Task<int?>>? ShowQuantityNumpadDialogHandler { get; set; }

    public PosViewModel(
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        ICustomerApiService customerApiService,
        ICategoryApiService categoryApiService,
        IProductApiService productApiService,
        IStockApiService stockApiService,
        ISalesInvoiceApiService salesInvoiceApiService,
        IToastService toastService,
        IPosSessionService sessionService)
    {
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _customerApiService = customerApiService;
        _categoryApiService = categoryApiService;
        _productApiService = productApiService;
        _stockApiService = stockApiService;
        _salesInvoiceApiService = salesInvoiceApiService;
        _toastService = toastService;
        _sessionService = sessionService;
        _sessionScope = sessionService.ScopeKey;
        _saveTimer.Tick += (_, _) => SaveWorkspace();
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(OrderNotes) or nameof(SelectedWarehouse) or nameof(SelectedBranch) or nameof(SelectedCustomer)) QueueSave();
        };

        _ = InitializeAsync();
    }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        if (!_restoring) _ = FilterWarehousesForBranchAsync();
    }

    partial void OnSelectedCustomerChanged(CustomerSummaryDto? value)
    {
        RecalculateTotals();
    }

    partial void OnSelectedPaymentMethodChanged(PaymentMethod value)
    {
        RecalculateTotals();
    }

    partial void OnOrderDiscountChanged(decimal value) => RecalculateTotals();
    partial void OnTaxAmountChanged(decimal value) => RecalculateTotals();

    partial void OnPaidAmountChanged(decimal value)
    {
        RecalculateTotals();
    }

    public async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            // 1. Branches
            var branchRes = await _branchApiService.GetAllAsync();
            if (branchRes.Success && branchRes.Data != null)
            {
                Branches = new ObservableCollection<BranchDto>(branchRes.Data);
                if (SelectedBranch == null && Branches.Count > 0)
                {
                    SelectedBranch = Branches[0];
                }
            }

            // 2. Warehouses (Showrooms)
            await FilterWarehousesForBranchAsync();

            // 3. Customers
            await LoadCustomersAsync();

            // 4. Categories
            var catRes = await _categoryApiService.GetAllAsync();
            if (catRes.Success && catRes.Data != null)
            {
                _allCategories = catRes.Data;
            }

            // 5. Products
            var prodRes = await _productApiService.GetAllAsync();
            if (prodRes.Success && prodRes.Data != null)
            {
                _allProducts = prodRes.Data;
                PopulateDefaultQuickItems();
            }

            // 6. Load Showroom Stock
            await LoadShowroomStocksAsync();
        });
        if (_restoring)
        {
            try
            {
                var state = _sessionService.Load();
                HeldOrders = new ObservableCollection<PosDraft>(state.Held);
                HasHeldOrder = HeldOrders.Count > 0;
                SelectedHeldOrder = HeldOrders.FirstOrDefault();
                if (state.Active is { } draft) await ApplyDraftAsync(draft);
            }
            catch (Exception)
            {
                _draftReadFailed = true;
                DraftStatus = "تعذر استعادة المسودة؛ لن يتم استبدال الملف المحفوظ. تحقق من التخزين قبل متابعة العمل.";
            }
            finally { _restoring = false; }
        }
    }

    public async Task LoadCustomersAsync()
    {
        var custRes = await _customerApiService.GetAllAsync(isActive: true);
        if (custRes.Success && custRes.Data != null)
        {
            Customers = new ObservableCollection<CustomerSummaryDto>(custRes.Data);
        }
    }

    private async Task LoadShowroomStocksAsync()
    {
        if (SelectedWarehouse == null || _allProducts == null || _allProducts.Count == 0) return;
        try
        {
            var stockRes = await _stockApiService.GetShowroomStocksByWarehouseAsync(SelectedWarehouse.Id);
            if (stockRes.Success && stockRes.Data != null)
            {
                var stockMap = stockRes.Data.ToDictionary(s => s.ProductId, s => s.Quantity);
                foreach (var p in _allProducts)
                {
                    p.ShowroomQuantity = stockMap.TryGetValue(p.Id, out int qty) ? qty : 0;
                }
            }
        }
        catch { }
    }

    private void PopulateDefaultQuickItems()
    {
        if (QuickItems.Count > 0) return;

        // Populate quick items with top frequent / unbarcoded / first few items
        var quicks = _allProducts.Take(12).Select(p => new QuickProductItem
        {
            ProductId = p.Id,
            Name = p.Name,
            Code = p.Code,
            Price = p.SalePrice,
            CategoryName = p.CategoryName,
            BarCode = p.DefaultBarCode ?? p.BarCodes?.FirstOrDefault()?.BarCode,
            ImageUrl = p.DefaultImageUrl ?? p.DefaultImage ?? p.Images?.FirstOrDefault(i => i.IsDefault)?.FullImageUrl
        }).ToList();

        QuickItems = new ObservableCollection<QuickProductItem>(quicks);
    }

    private async Task FilterWarehousesForBranchAsync()
    {
        var whRes = await _warehouseApiService.GetAllAsync(branchId: SelectedBranch?.Id);
        if (whRes.Success && whRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
            if (SelectedWarehouse == null || !Warehouses.Any(w => w.Id == SelectedWarehouse.Id))
            {
                var showroom = Warehouses.FirstOrDefault(w => w.TypeName.Contains("عرض") || w.TypeName.Contains("Showroom"));
                SelectedWarehouse = showroom ?? Warehouses.FirstOrDefault();
            }
        }
    }

    [RelayCommand]
    private Task RefreshAsync() => InitializeAsync();

    [RelayCommand]
    private void ToggleQuickItems()
    {
        IsQuickItemsVisible = !IsQuickItemsVisible;
    }

    private readonly Queue<string> _barcodeQueue = new();
    [ObservableProperty] private bool _isScanning;

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task AddBarcodeAsync()
    {
        if (IsLoading || string.IsNullOrWhiteSpace(BarcodeQuery)) return;
        _barcodeQueue.Enqueue(BarcodeQuery.Trim());
        BarcodeQuery = string.Empty;
        if (IsScanning) return;
        IsScanning = true;
        try
        {
            while (_barcodeQueue.TryDequeue(out var code))
            {
                var product = _allProducts.FirstOrDefault(p =>
                    string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Name, code, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.DefaultBarCode, code, StringComparison.OrdinalIgnoreCase) ||
                    (p.BarCodes?.Any(b => string.Equals(b.BarCode, code, StringComparison.OrdinalIgnoreCase)) ?? false));
                if (product == null)
                {
                    var response = await _productApiService.GetByBarCodeAsync(code);
                    product = response.Success ? response.Data : null;
                }
                if (product == null)
                {
                    ErrorMessage = $"تعذر إضافة الباركود {code}. تحقق من الكود والاتصال ثم أعد مسحه.";
                    _toastService.ShowWarning(ErrorMessage, "لم يضف الصنف");
                    continue;
                }
                if (!_allProducts.Any(p => p.Id == product.Id)) _allProducts.Add(product);
                var barcode = product.BarCodes?.FirstOrDefault(b => string.Equals(b.BarCode, code, StringComparison.OrdinalIgnoreCase));
                AddProductToCart(product, barcode?.Id, barcode?.BarCode ?? code);
            }
        }
        catch (Exception)
        {
            ErrorMessage = "تعذر إكمال قراءة الأصناف. راجع السلة وأعد مسح الأصناف التي لم تُضف.";
            _barcodeQueue.Clear();
        }
        finally { IsScanning = false; SaveWorkspace(); }
    }
    [RelayCommand]
    private void AddQuickItem(QuickProductItem? quick)
    {
        if (quick == null) return;
        var product = _allProducts.FirstOrDefault(p => p.Id == quick.ProductId);
        if (product != null)
        {
            AddProductToCart(product, null, quick.BarCode);
        }
    }

    [RelayCommand]
    private async Task OpenProductSearchDialogAsync()
    {
        if (ShowProductSearchDialogHandler != null)
        {
            var product = await ShowProductSearchDialogHandler(_allProducts, _allCategories);
            if (product != null)
            {
                var defaultBarcode = product.BarCodes?.FirstOrDefault();
                AddProductToCart(product, defaultBarcode?.Id, defaultBarcode?.BarCode);
                RequestBarcodeFocus?.Invoke();
            }
        }
    }

    [RelayCommand]
    private async Task OpenQuickCustomerDialogAsync()
    {
        if (ShowQuickCustomerDialogHandler != null)
        {
            var newCustomer = await ShowQuickCustomerDialogHandler();
            if (newCustomer != null)
            {
                await LoadCustomersAsync();
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == newCustomer.Id) ?? newCustomer;
                _toastService.ShowSuccess($"تمت إضافة واختيار العميل '{newCustomer.Name}' بنجاح", "عميل جديد");
                RequestBarcodeFocus?.Invoke();
            }
        }
    }

    [RelayCommand]
    private async Task OpenQuantityNumpadAsync(CartItemModel? item)
    {
        if (item == null) return;
        if (ShowQuantityNumpadDialogHandler != null)
        {
            var newQty = await ShowQuantityNumpadDialogHandler(item);
            if (newQty.HasValue && newQty.Value > 0)
            {
                SetItemQuantity(item, newQty.Value);
            }
        }
    }

    public void SetItemQuantity(CartItemModel? item, int quantity)
    {
        if (item == null) return;
        if (quantity <= 0)
        {
            CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }
        RecalculateTotals();
        RequestBarcodeFocus?.Invoke();
    }

    public void AddProductToCart(ProductDto product, Guid? barcodeId = null, string? barcodeStr = null)
    {
        var existing = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            var item = new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name, // اسم الصنف العام دائماً من جدول المنتجات
                Code = product.Code,
                ProductBarCodeId = barcodeId,
                BarCode = barcodeStr ?? product.DefaultBarCode ?? product.BarCodes?.FirstOrDefault()?.BarCode,
                UnitPrice = product.SalePrice,
                Quantity = 1,
                DiscountAmount = 0,
                UnitName = product.Units?.FirstOrDefault(u => u.IsDefault)?.UnitName ?? "قطعة",
                ImageUrl = product.DefaultImageUrl ?? product.DefaultImage ?? product.Images?.FirstOrDefault(i => i.IsDefault)?.FullImageUrl
            };
            item.PropertyChanged += (s, e) => RecalculateTotals();
            CartItems.Add(item);
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void IncrementQuantity(CartItemModel? item)
    {
        if (item != null)
        {
            item.Quantity++;
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void DecrementQuantity(CartItemModel? item)
    {
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                CartItems.Remove(item);
            }
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void RemoveCartItem(CartItemModel? item)
    {
        if (item != null)
        {
            CartItems.Remove(item);
            RecalculateTotals();
            RequestBarcodeFocus?.Invoke();
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        if (IsLoading || IsScanning || CartItems.Count == 0) return;
        if (ConfirmActionHandler?.Invoke("إفراغ السلة", "سيتم حذف أصناف الفاتورة الحالية. هل تريد المتابعة؟") != true) return;
        ResetCart();
        SaveWorkspace();
    }

    private void ResetCart()
    {
        _restoring = true;
        CartItems.Clear();
        SelectedCustomer = null;
        SelectedPaymentMethod = PaymentMethod.Cash;
        OrderDiscount = TaxAmount = PaidAmount = 0;
        OrderNotes = string.Empty;
        ErrorMessage = null;
        SuccessMessage = null;
        _activeDraftId = Guid.NewGuid();
        _restoring = false;
        RecalculateTotals();
        RequestBarcodeFocus?.Invoke();
    }

    private PosDraft CaptureDraft() => new()
    {
        Id = _activeDraftId, BranchId = SelectedBranch?.Id, WarehouseId = SelectedWarehouse?.Id,
        CustomerId = SelectedCustomer?.Id, CustomerName = SelectedCustomer?.Name,
        Items = CartItems.ToList(), Discount = OrderDiscount, Tax = TaxAmount,
        Paid = PaidAmount, PaymentMethod = SelectedPaymentMethod, Notes = OrderNotes
    };

    private void QueueSave()
    {
        if (_restoring || _draftReadFailed) return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public override void SaveWorkspace()
    {
        _saveTimer.Stop();
        if (_restoring || _draftReadFailed || _sessionScope == null) return;
        try
        {
            _sessionService.Save(_sessionScope, new PosSessionState
            {
                Active = CartItems.Count > 0 ? CaptureDraft() : null,
                Held = HeldOrders.ToList()
            }.Copy());
            DraftStatus = "المسودة محفوظة على هذا الجهاز";
        }
        catch (Exception)
        {
            DraftStatus = "تعذر حفظ المسودة على الجهاز. أبقِ الشاشة مفتوحة وأعد المحاولة.";
        }
    }

    [RelayCommand]
    private void HoldOrder()
    {
        if (IsLoading || IsScanning || CartItems.Count == 0 || _draftReadFailed) return;
        var snapshot = new PosSessionState { Active = CaptureDraft() }.Copy().Active!;
        HeldOrders.Add(snapshot);
        SelectedHeldOrder = snapshot;
        HasHeldOrder = true;
        ResetCart();
        SaveWorkspace();
        _toastService.ShowInfo("يمكنك اختيار أي طلب من قائمة الطلبات المعلقة لاستعادته.", "تم تعليق الطلب");
    }

    [RelayCommand]
    private async Task RestoreHeldOrderAsync()
    {
        if (IsLoading || IsScanning || SelectedHeldOrder is not { } draft || _draftReadFailed) return;
        if (CartItems.Count > 0)
        {
            _toastService.ShowWarning("علّق السلة الحالية أولاً أو أفرغها قبل استعادة طلب آخر.", "السلة تحتوي أصنافًا");
            return;
        }
        await ExecuteAsync(async () =>
        {
            try
            {
                await ApplyDraftAsync(draft);
                HeldOrders.Remove(draft);
                SelectedHeldOrder = HeldOrders.FirstOrDefault();
                HasHeldOrder = HeldOrders.Count > 0;
            }
            finally { _restoring = false; }
        });
        SaveWorkspace();
        RequestBarcodeFocus?.Invoke();
    }

    private async Task ApplyDraftAsync(PosDraft draft)
    {
        _restoring = true;
        var branch = Branches.FirstOrDefault(b => b.Id == draft.BranchId);
        if (branch == null) throw new InvalidOperationException("تعذر تحميل فرع المسودة. حدّث البيانات وأعد المحاولة.");
        SelectedBranch = branch;
        await FilterWarehousesForBranchAsync();
        var warehouse = Warehouses.FirstOrDefault(w => w.Id == draft.WarehouseId);
        var customer = Customers.FirstOrDefault(c => c.Id == draft.CustomerId);
        if (warehouse == null || (draft.CustomerId != null && customer == null))
            throw new InvalidOperationException("تعذر تحميل مخزن أو عميل المسودة؛ لم تُحذف الأصناف المحفوظة.");
        SelectedWarehouse = warehouse;
        SelectedCustomer = customer;
        CartItems = new ObservableCollection<CartItemModel>(draft.Items);
        foreach (var item in CartItems) item.PropertyChanged += (_, _) => RecalculateTotals();
        OrderDiscount = draft.Discount;
        TaxAmount = draft.Tax;
        SelectedPaymentMethod = draft.PaymentMethod;
        PaidAmount = draft.Paid;
        OrderNotes = draft.Notes;
        _activeDraftId = draft.Id;
        RecalculateTotals();
        DraftStatus = "تمت استعادة مسودة البيع";
    }
    [RelayCommand]
    private void SetQuickCash(object? parameter)
    {
        if (parameter is string valStr && decimal.TryParse(valStr, out decimal val))
        {
            if (val == 0)
            {
                PaidAmount = GrandTotal;
            }
            else
            {
                PaidAmount = val;
            }
        }
        else if (parameter is decimal num)
        {
            PaidAmount = (num == 0) ? GrandTotal : num;
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = CartItems.Sum(c => c.LineTotal);
        TotalItemCount = CartItems.Sum(c => c.Quantity);
        GrandTotal = Math.Max(0, SubTotal - OrderDiscount + TaxAmount);
        PaymentShortfall = SelectedPaymentMethod == PaymentMethod.Cash ? Math.Max(0, GrandTotal - PaidAmount) : 0;

        if (SelectedPaymentMethod == PaymentMethod.Credit)
        {
            // For debt / credit sales:
            CustomerDebtAmount = Math.Max(0, GrandTotal - PaidAmount);
            ChangeAmount = Math.Max(0, PaidAmount - GrandTotal);
        }
        else
        {
            CustomerDebtAmount = 0;
            ChangeAmount = SelectedPaymentMethod == PaymentMethod.Cash ? Math.Max(0, PaidAmount - GrandTotal) : 0;
        }
        QueueSave();
    }

    [RelayCommand]
    private async Task SubmitSaleAsync()
    {
        if (_draftReadFailed) { ErrorMessage = DraftStatus; return; }
        if (IsScanning) { ErrorMessage = "انتظر اكتمال إضافة الأصناف قبل إتمام البيع."; return; }
        if (PaidAmount < 0 || OrderDiscount < 0 || TaxAmount < 0 || OrderDiscount > SubTotal || CartItems.Any(i => i.Quantity <= 0 || i.UnitPrice < 0))
        {
            ErrorMessage = "راجع الكميات والأسعار والخصم والمبلغ المستلم؛ لا تقبل القيم السالبة أو خصمًا أكبر من المجموع.";
            return;
        }
        if (SelectedPaymentMethod == PaymentMethod.Cash && PaidAmount < GrandTotal)
        {
            ErrorMessage = $"المبلغ المستلم أقل من الإجمالي بمقدار {PaymentShortfall:N2} د.ل. أدخل المستلم أو اختر المبلغ كاملًا (F8).";
            _toastService.ShowWarning(ErrorMessage, "الدفع غير مكتمل");
            return;
        }
        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب اختيار الفرع أولاً";
            _toastService.ShowWarning(ErrorMessage, "تنبيه");
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يجب اختيار صالة العرض أو المخزن للخصم منه";
            _toastService.ShowWarning(ErrorMessage, "تنبيه");
            return;
        }

        if (CartItems.Count == 0)
        {
            ErrorMessage = "سلة المبيعات فارغة! يرجى مسح باركود الأصناف للطلب.";
            _toastService.ShowWarning(ErrorMessage, "سلة فارغة");
            RequestBarcodeFocus?.Invoke();
            return;
        }

        if (SelectedPaymentMethod == PaymentMethod.Credit)
        {
            if (SelectedCustomer == null)
            {
                ErrorMessage = "لا يمكن تسجيل البيع آجل (دين) بدون تحديد الزبون. يرجى اختيار عميل أو إضافة عميل جديد.";
                _toastService.ShowWarning(ErrorMessage, "تنبيه البيع الآجل");
                return;
            }

            // Check credit limit if specified
            if (SelectedCustomer.CreditLimit > 0)
            {
                decimal newBalance = SelectedCustomer.CurrentBalance + CustomerDebtAmount;
                if (newBalance > SelectedCustomer.CreditLimit)
                {
                    // Warn cashier about exceeding credit limit
                    _toastService.ShowWarning($"تنبيه: سيصل رصيد ديون العميل إلى {newBalance:N2} د.ل متجاوزاً سقف الائتمان ({SelectedCustomer.CreditLimit:N2} د.ل)", "تجاوز سقف الائتمان");
                }
            }
        }

        ErrorMessage = null;
        SuccessMessage = null;

        await ExecuteAsync(async () =>
        {
            // The server enforces unique invoice numbers per tenant. Retrying a draft
            // after a lost response must not create another stock movement.
            var invoiceNum = $"POS-{_activeDraftId:N}";
            var isCredit = (SelectedPaymentMethod == PaymentMethod.Credit);

            var req = new CreateSalesInvoiceRequest
            {
                InvoiceNumber = invoiceNum,
                BranchId = SelectedBranch.Id,
                WarehouseId = SelectedWarehouse.Id,
                CustomerId = SelectedCustomer?.Id,
                InvoiceDate = DateTime.UtcNow,
                Status = (isCredit && CustomerDebtAmount > 0) ? InvoiceStatus.Issued : InvoiceStatus.Paid,
                PaymentMethod = SelectedPaymentMethod,
                SubTotal = SubTotal,
                DiscountAmount = OrderDiscount,
                TaxAmount = TaxAmount,
                TotalAmount = GrandTotal,
                PaidAmount = SelectedPaymentMethod == PaymentMethod.Card ? GrandTotal : Math.Min(PaidAmount, GrandTotal),
                Notes = OrderNotes ?? (isCredit ? $"فاتورة بيع آجل (دين) - متبقي دين: {CustomerDebtAmount:N2} د.ل" : null),
                Items = CartItems.Select(c => new CreateSalesInvoiceItemRequest
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    ProductBarCodeId = c.ProductBarCodeId,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    DiscountAmount = c.DiscountAmount
                }).ToList()
            };

            var res = await _salesInvoiceApiService.CreateAsync(req);
            if (res.Success && res.Data != null)
            {
                var change = ChangeAmount;
                var debt = CustomerDebtAmount;
                ResetCart();
                SaveWorkspace();

                string msg;
                if (isCredit && debt > 0)
                {
                    msg = $"تم تسجيل فاتورة البيع الآجل (دين) بنجاح! رقم: {invoiceNum} (المتبقي كدين على العميل: {debt:N2} د.ل)";
                }
                else
                {
                    msg = $"تم إتمام البيع بنجاح! رقم الفاتورة: {invoiceNum}" + (change > 0 ? $" (المتبقي للعميل: {change:N2} د.ل)" : string.Empty);
                }

                SuccessMessage = $"🎉 {msg}";
                _toastService.ShowSuccess(msg, "إتمام الفاتورة");
                RequestBarcodeFocus?.Invoke();
            }
            else
            {
                ErrorMessage = res.ErrorCode == "NETWORK_ERROR"
                    ? $"لم يصل تأكيد الحفظ. راجع الفاتورة {invoiceNum} في المبيعات قبل إعادة المحاولة؛ السلة محفوظة."
                    : res.Message ?? "فشل إتمام عملية البيع";
                _toastService.ShowError(ErrorMessage, "خطأ في الفاتورة");
            }
        });
    }
}
