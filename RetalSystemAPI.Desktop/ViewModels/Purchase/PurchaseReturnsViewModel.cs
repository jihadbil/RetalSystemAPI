using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Purchase;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Purchase;

public partial class PurchaseReturnsViewModel : BaseViewModel
{
    private int _loadVersion;

    private readonly IPurchaseReturnApiService _purchaseReturnApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly ISupplierApiService _supplierApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<PurchaseReturnSummaryDto> _returns = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<SupplierSummaryDto> _suppliers = new();

    [ObservableProperty]
    private SupplierSummaryDto? _selectedSupplier;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private PurchaseReturnReason? _selectedReason;

    [ObservableProperty]
    private PaymentMethod? _selectedPaymentMethod;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<PurchaseReturnSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public PurchaseReturnsViewModel(
        IPurchaseReturnApiService purchaseReturnApiService,
        IBranchApiService branchApiService,
        ISupplierApiService supplierApiService,
        IWarehouseApiService warehouseApiService)
    {
        _purchaseReturnApiService = purchaseReturnApiService;
        _branchApiService = branchApiService;
        _supplierApiService = supplierApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadLookupDataAsync();
        await LoadReturnsAsync();
    }

    private async Task LoadLookupDataAsync()
    {
        var branchRes = await _branchApiService.GetAllAsync();
        if (branchRes.Success && branchRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchRes.Data);
        }

        var supRes = await _supplierApiService.GetAllAsync();
        if (supRes.Success && supRes.Data != null)
        {
            Suppliers = new ObservableCollection<SupplierSummaryDto>(supRes.Data);
        }

        var whRes = await _warehouseApiService.GetAllAsync();
        if (whRes.Success && whRes.Data != null)
        {
            // في مرتجع المشتريات تظهر المستودعات الرئيسية (المخازن)
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data.Where(w => w.Type == WarehouseType.Storge));
        }
    }

    [RelayCommand]
    public async Task LoadReturnsAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            var res = await _purchaseReturnApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                branchId: SelectedBranch?.Id,
                warehouseId: SelectedWarehouse?.Id,
                supplierId: SelectedSupplier?.Id,
                reason: SelectedReason,
                paymentMethod: SelectedPaymentMethod,
                search: SearchQuery);

            if (version != _loadVersion) return;

            if (res.Success && res.Data != null)
            {
                Returns = new ObservableCollection<PurchaseReturnSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadReturnsAsync();
                    return;
                }
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل مرتجعات المشتريات";
            }
        }, isCurrent: () => version == _loadVersion);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadReturnsAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadReturnsAsync();
        }
    }

    [RelayCommand]
    private async Task CreateReturnAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadReturnsAsync();
        }
    }

    [RelayCommand]
    private async Task ViewReturnDetailsAsync(object? parameter)
    {
        if (parameter is PurchaseReturnSummaryDto ret && OpenDialogHandler != null)
        {
            await OpenDialogHandler(ret);
        }
    }

    [RelayCommand]
    private async Task DeleteReturnAsync(object? parameter)
    {
        if (parameter is not PurchaseReturnSummaryDto ret) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المرتجع", $"هل أنت متأكد من حذف مرتجع المشتريات رقم '{ret.ReturnNumber}' وإعادة البضاعة للمخزن؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _purchaseReturnApiService.DeleteAsync(ret.Id);
            if (res.Success)
            {
                await LoadReturnsAsync();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }

    partial void OnSelectedSupplierChanged(SupplierSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }

    partial void OnSelectedReasonChanged(PurchaseReturnReason? value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }

    partial void OnSelectedPaymentMethodChanged(PaymentMethod? value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadReturnsAsync);
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadReturnsAsync();
    }
}

public partial class PurchaseReturnFormViewModel : BaseViewModel
{
    private readonly IPurchaseReturnApiService _purchaseReturnApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly ISupplierApiService _supplierApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductApiService _productApiService;
    private readonly IPurchaseInvoiceApiService _purchaseInvoiceApiService;

    [ObservableProperty]
    private Guid? _returnId;

    [ObservableProperty]
    private string _returnNumber = $"PRET-{DateTime.Now:yyyyMMddHHmmss}";

    [ObservableProperty]
    private DateTime _returnDate = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<SupplierSummaryDto> _suppliers = new();

    [ObservableProperty]
    private SupplierSummaryDto? _selectedSupplier;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private ObservableCollection<PurchaseInvoiceSummaryDto> _supplierInvoices = new();

    [ObservableProperty]
    private PurchaseInvoiceSummaryDto? _selectedInvoice;

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private PurchaseReturnReason _reason = PurchaseReturnReason.Defective;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    // بحث تدريجي في الأصناف بدل تحميل الكتالوج كاملاً
    [ObservableProperty]
    private string _productSearchTerm = string.Empty;

    /// <summary>مطابقة الباركود تماماً — يبحث بالتطابق التام مع رقم الباركود</summary>
    [ObservableProperty]
    private bool _exactBarcodeMatch = false;

    [ObservableProperty]
    private ProductDto? _selectedProductToAdd;

    [ObservableProperty]
    private ObservableCollection<ProductBarCodeDto> _availableBarcodes = new();

    [ObservableProperty]
    private ProductBarCodeDto? _selectedBarcodeToAdd;

    [ObservableProperty]
    private decimal _quantityToAdd = 1;

    [ObservableProperty]
    private decimal _unitPriceToAdd;

    [ObservableProperty]
    private ObservableCollection<CreatePurchaseReturnItemRequest> _items = new();

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isViewOnly;

    public Action? CloseWindowHandler { get; set; }

    public PurchaseReturnFormViewModel(
        IPurchaseReturnApiService purchaseReturnApiService,
        IBranchApiService branchApiService,
        ISupplierApiService supplierApiService,
        IWarehouseApiService warehouseApiService,
        IProductApiService productApiService,
        IPurchaseInvoiceApiService purchaseInvoiceApiService)
    {
        _purchaseReturnApiService = purchaseReturnApiService;
        _branchApiService = branchApiService;
        _supplierApiService = supplierApiService;
        _warehouseApiService = warehouseApiService;
        _productApiService = productApiService;
        _purchaseInvoiceApiService = purchaseInvoiceApiService;
    }

    public async Task InitializeForCreateAsync()
    {
        IsViewOnly = false;
        ReturnNumber = $"PRET-{DateTime.Now:yyyyMMddHHmmss}";
        ReturnDate = DateTime.Now;
        Items.Clear();
        TotalAmount = 0;

        await LoadLookupsAsync();
    }

    /// <summary>تهيئة النموذج محملاً بفاتورة مشتريات أصلية محددة مسبقاً (من زر «مرتجع» على بند فاتورة مغلقة)</summary>
    public async Task InitializeForInvoiceAsync(Guid invoiceId, CreatePurchaseReturnItemRequest? prefilledItem = null)
    {
        await LoadLookupsAsync();

        IsViewOnly = false;
        ReturnNumber = $"PRET-{DateTime.Now:yyyyMMddHHmmss}";
        ReturnDate = DateTime.Now;
        Items.Clear();
        TotalAmount = 0;
        Notes = string.Empty;

        var res = await _purchaseInvoiceApiService.GetByIdAsync(invoiceId);
        if (res.Success && res.Data != null)
        {
            var invoice = res.Data;

            // المرتجع يخرج حصراً من نفس فرع ومستودع الفاتورة ولموردها
            SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == invoice.SupplierId);
            SelectedBranch = Branches.FirstOrDefault(b => b.Id == invoice.BranchId);
            SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == invoice.WarehouseId && w.Type == WarehouseType.Storge);

            var summary = new PurchaseInvoiceSummaryDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                SupplierId = invoice.SupplierId,
                Status = invoice.Status
            };
            SupplierInvoices = new ObservableCollection<PurchaseInvoiceSummaryDto> { summary };
            SelectedInvoice = summary;

            if (prefilledItem != null)
            {
                Items.Add(prefilledItem);
                CalculateTotal();
            }
        }
        else
        {
            ErrorMessage = res.Message ?? "فشل تحميل فاتورة المشتريات الأصلية";
        }
    }

    public async Task InitializeForViewAsync(Guid returnId)
    {
        IsViewOnly = true;
        ReturnId = returnId;

        await ExecuteAsync(async () =>
        {
            var res = await _purchaseReturnApiService.GetByIdAsync(returnId);
            if (res.Success && res.Data != null)
            {
                var ret = res.Data;
                ReturnNumber = ret.ReturnNumber;
                ReturnDate = ret.ReturnDate;
                Reason = ret.Reason;
                PaymentMethod = ret.PaymentMethod;
                Notes = ret.Notes;
                TotalAmount = ret.TotalAmount;

                Items = new ObservableCollection<CreatePurchaseReturnItemRequest>(
                    ret.Items.Select(i => new CreatePurchaseReturnItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductBarCodeId = i.ProductBarCodeId,
                        BarcodeTitle = i.BarcodeTitle ?? i.BarcodeValue,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Notes = i.Notes
                    }));
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل المرتجع";
            }
        });
    }

    private async Task LoadLookupsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var branchRes = await _branchApiService.GetAllAsync();
            if (branchRes.Success && branchRes.Data != null)
            {
                Branches = new ObservableCollection<BranchDto>(branchRes.Data);
                SelectedBranch = Branches.FirstOrDefault();
            }

            var supRes = await _supplierApiService.GetAllAsync();
            if (supRes.Success && supRes.Data != null)
            {
                Suppliers = new ObservableCollection<SupplierSummaryDto>(supRes.Data);
                SelectedSupplier = Suppliers.FirstOrDefault();
            }

            var whRes = await _warehouseApiService.GetAllAsync();
            if (whRes.Success && whRes.Data != null)
            {
                // مرتجع المشتريات يتم حصرياً من المستودعات الرئيسية (المخازن)
                Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data.Where(w => w.Type == WarehouseType.Storge));
                SelectedWarehouse = Warehouses.FirstOrDefault();
            }

            // الأصناف لا تُجلب كاملة؛ تُبحث تدريجياً بالاسم أو الباركود من الخادم
        });
    }

    partial void OnProductSearchTermChanged(string value)
    {
        // بحث مؤجل (Debounce) لتفادي إغراق الخادم بطلب لكل حرف
        _ = DebounceSearchAsync(SearchProductsAsync);
    }

    private async Task SearchProductsAsync()
    {
        var term = ProductSearchTerm?.Trim() ?? string.Empty;
        if (term.Length == 0)
        {
            AvailableProducts.Clear();
            return;
        }

        // مطابقة تامة: استعلام مباشر بنقطة الباركود الدقيقة بدل البحث بالاحتواء
        if (ExactBarcodeMatch)
        {
            var exactRes = await _productApiService.GetByBarCodeAsync(term);
            if (exactRes.Success && exactRes.Data != null)
            {
                AvailableProducts = new ObservableCollection<ProductDto> { exactRes.Data };
            }
            else
            {
                AvailableProducts.Clear();
                ErrorMessage = $"لم يتم العثور على صنف بباركود مطابق تماماً: {term}";
            }
            return;
        }

        var res = await _productApiService.SearchAsync(term);
        if (res.Success && res.Data != null)
        {
            AvailableProducts = new ObservableCollection<ProductDto>(res.Data);
        }
        else if (!res.Success)
        {
            ErrorMessage = res.Message;
        }
    }

    partial void OnSelectedSupplierChanged(SupplierSummaryDto? value)
    {
        _ = LoadSupplierInvoicesAsync(value?.Id);
    }

    private async Task LoadSupplierInvoicesAsync(Guid? supplierId)
    {
        if (!supplierId.HasValue)
        {
            SupplierInvoices.Clear();
            SelectedInvoice = null;
            return;
        }

        var res = await _purchaseInvoiceApiService.GetPagedAsync(pageSize: 50, supplierId: supplierId);
        if (res.Success && res.Data != null)
        {
            SupplierInvoices = new ObservableCollection<PurchaseInvoiceSummaryDto>(res.Data.Items);
        }
        else
        {
            SupplierInvoices.Clear();
        }
    }

    partial void OnSelectedProductToAddChanged(ProductDto? value)
    {
        if (value != null)
        {
            UnitPriceToAdd = value.CostPrice > 0 ? value.CostPrice : value.SalePrice;
            if (value.BarCodes != null && value.BarCodes.Any())
            {
                AvailableBarcodes = new ObservableCollection<ProductBarCodeDto>(value.BarCodes);
                SelectedBarcodeToAdd = AvailableBarcodes.FirstOrDefault();
            }
            else
            {
                AvailableBarcodes.Clear();
                SelectedBarcodeToAdd = null;
            }
        }
        else
        {
            UnitPriceToAdd = 0;
            AvailableBarcodes.Clear();
            SelectedBarcodeToAdd = null;
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedProductToAdd == null)
        {
            ErrorMessage = "يرجى اختيار الصنف أولاً";
            return;
        }

        if (QuantityToAdd <= 0)
        {
            ErrorMessage = "يجب أن تكون الكمية المرتجعة أكبر من صفر";
            return;
        }

        if (UnitPriceToAdd < 0)
        {
            ErrorMessage = "سعر الوحدة يجب ألا يكون سالباً";
            return;
        }

        var barcode = SelectedBarcodeToAdd ?? SelectedProductToAdd.BarCodes?.FirstOrDefault();

        var existingItem = Items.FirstOrDefault(i =>
            i.ProductId == SelectedProductToAdd.Id &&
            i.ProductBarCodeId == barcode?.Id);

        if (existingItem != null)
        {
            existingItem.Quantity += QuantityToAdd;
            existingItem.UnitPrice = UnitPriceToAdd;
        }
        else
        {
            Items.Add(new CreatePurchaseReturnItemRequest
            {
                ProductId = SelectedProductToAdd.Id,
                ProductName = SelectedProductToAdd.Name,
                ProductBarCodeId = barcode?.Id,
                BarcodeTitle = barcode?.Title ?? barcode?.BarCode ?? "افتراضي",
                Quantity = QuantityToAdd,
                UnitPrice = UnitPriceToAdd
            });
        }

        CalculateTotal();
        ErrorMessage = null;
        QuantityToAdd = 1;
    }

    [RelayCommand]
    private void RemoveItem(CreatePurchaseReturnItemRequest? item)
    {
        if (item != null && Items.Contains(item))
        {
            Items.Remove(item);
            CalculateTotal();
        }
    }

    private void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.LineTotal);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ReturnNumber))
        {
            ErrorMessage = "يرجى إدخال رقم إشعار المرتجع";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "يرجى اختيار الفرع";
            return;
        }

        if (SelectedSupplier == null)
        {
            ErrorMessage = "يرجى اختيار المورد";
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يرجى اختيار المستودع الرئيسي (المخزن)";
            return;
        }

        if (Items.Count == 0)
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل للمرتجع";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreatePurchaseReturnRequest
            {
                ReturnNumber = ReturnNumber.Trim(),
                ReturnDate = ReturnDate,
                PurchaseInvoiceId = SelectedInvoice?.Id,
                SupplierId = SelectedSupplier.Id,
                BranchId = SelectedBranch.Id,
                WarehouseId = SelectedWarehouse.Id,
                PaymentMethod = PaymentMethod,
                Reason = Reason,
                Notes = Notes,
                Items = Items.ToList()
            };

            var res = await _purchaseReturnApiService.CreateAsync(req);
            if (res.Success)
            {
                CloseWindowHandler?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل حفظ مرتجع المشتريات";
            }
        });
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseWindowHandler?.Invoke();
    }
}
