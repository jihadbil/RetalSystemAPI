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
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.Services.Sales;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Sales;

public partial class SalesReturnsViewModel : BaseViewModel
{
    private int _loadVersion;

    private readonly ISalesReturnApiService _salesReturnApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<SalesReturnSummaryDto> _returns = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private SalesReturnReason? _selectedReason;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<SalesReturnSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public SalesReturnsViewModel(
        ISalesReturnApiService salesReturnApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService)
    {
        _salesReturnApiService = salesReturnApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAndWarehousesAsync();
        await LoadReturnsAsync();
    }

    private async Task LoadBranchesAndWarehousesAsync()
    {
        var branchRes = await _branchApiService.GetAllAsync();
        if (branchRes.Success && branchRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchRes.Data);
        }

        var whRes = await _warehouseApiService.GetAllAsync();
        if (whRes.Success && whRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
        }
    }

    [RelayCommand]
    public async Task LoadReturnsAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            var res = await _salesReturnApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                branchId: SelectedBranch?.Id,
                warehouseId: SelectedWarehouse?.Id,
                reason: SelectedReason,
                search: SearchQuery);
            if (version != _loadVersion) return;

            if (res.Success && res.Data != null)
            {
                Returns = new ObservableCollection<SalesReturnSummaryDto>(res.Data.Items);
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
                ErrorMessage = res.Message ?? "فشل تحميل مرتجعات المبيعات";
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
        if (parameter is SalesReturnSummaryDto ret && OpenDialogHandler != null)
        {
            await OpenDialogHandler(ret);
        }
    }

    [RelayCommand]
    private async Task DeleteReturnAsync(object? parameter)
    {
        if (parameter is not SalesReturnSummaryDto ret) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المرتجع", $"هل أنت متأكد من حذف المرتجع رقم '{ret.ReturnNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _salesReturnApiService.DeleteAsync(ret.Id);
            if (res.Success) await LoadReturnsAsync();
            else ErrorMessage = res.Message;
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
    partial void OnSelectedReasonChanged(SalesReturnReason? value)
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

public partial class SalesReturnFormViewModel : BaseViewModel
{
    private readonly ISalesReturnApiService _salesReturnApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly ICustomerApiService _customerApiService;
    private readonly IProductApiService _productApiService;
    private readonly ISalesInvoiceApiService _salesInvoiceApiService;
    private readonly Core.Auth.AuthStateService _authStateService;

    [ObservableProperty]
    private Guid? _returnId;

    [ObservableProperty]
    private string _returnNumber = string.Empty;

    [ObservableProperty]
    private DateTime _returnDate = DateTime.Now;

    [ObservableProperty]
    private SalesReturnReason _reason = SalesReturnReason.CustomerChangedMind;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private ObservableCollection<CustomerSummaryDto> _customers = new();

    [ObservableProperty]
    private CustomerSummaryDto? _selectedCustomer;

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
    private int _quantityToAdd = 1;

    [ObservableProperty]
    private decimal _unitPriceToAdd;

    [ObservableProperty]
    private ObservableCollection<CreateSalesReturnItemRequest> _items = new();

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isViewOnly;

    // ── الربط بالفاتورة الأصلية ─────────────────────────────
    [ObservableProperty]
    private ObservableCollection<SalesInvoiceSummaryDto> _invoices = new();

    [ObservableProperty]
    private SalesInvoiceSummaryDto? _selectedInvoice;

    [ObservableProperty]
    private string? _invoiceSearchTerm;

    [ObservableProperty]
    private bool _isInvoicelessMode;

    /// <summary>بنود الفاتورة المحددة كمرجع للكميات المسموح إرجاعها لكل صنف/باركود</summary>
    private List<SalesInvoiceItemDto> _selectedInvoiceItems = new();

    public bool CanReturnWithoutInvoice => _authStateService.HasPermission(Constants.Permissions.SalesReturns.ReturnWithoutInvoice);

    public bool HasOriginalInvoice => SelectedInvoice != null || IsInvoicelessMode == false && ReturnId.HasValue;

    public Action? CloseWindowHandler { get; set; }

    public SalesReturnFormViewModel(
        ISalesReturnApiService salesReturnApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        ICustomerApiService customerApiService,
        IProductApiService productApiService,
        ISalesInvoiceApiService salesInvoiceApiService,
        Core.Auth.AuthStateService authStateService)
    {
        _salesReturnApiService = salesReturnApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _customerApiService = customerApiService;
        _productApiService = productApiService;
        _salesInvoiceApiService = salesInvoiceApiService;
        _authStateService = authStateService;
    }

    partial void OnSelectedProductToAddChanged(ProductDto? value)
    {
        if (value != null)
        {
            UnitPriceToAdd = value.SalePrice;
        }
    }

    partial void OnInvoiceSearchTermChanged(string? value)
    {
        _ = DebounceSearchAsync(SearchInvoicesAsync);
    }

    private async Task SearchInvoicesAsync()
    {
        if (IsViewOnly) return;

        var term = InvoiceSearchTerm?.Trim();
        if (string.IsNullOrWhiteSpace(term))
        {
            Invoices.Clear();
            return;
        }

        var res = await _salesInvoiceApiService.GetPagedAsync(pageNumber: 1, pageSize: 50, search: term);
        if (res.Success && res.Data != null)
        {
            // استبعاد الفواتير الملغاة والباطلة من الاختيار كفاتورة أصلية
            Invoices = new ObservableCollection<SalesInvoiceSummaryDto>(
                res.Data.Items.Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Voided));

            // إعادة ربط التحديد الحالي بالكائن الجديد المطابق بعد استبدال القائمة (كي لا يظهر الكومبو فارغاً)
            if (SelectedInvoice != null)
            {
                var matched = Invoices.FirstOrDefault(i => i.Id == SelectedInvoice.Id);
                if (matched != null)
                {
                    SelectedInvoice = matched;
                }
            }
        }
    }

    partial void OnSelectedInvoiceChanged(SalesInvoiceSummaryDto? value)
    {
        if (IsViewOnly) return;
        if (value == null) return;

        _ = LoadInvoiceDetailsAsync(value.Id);
    }

    private async Task LoadInvoiceDetailsAsync(Guid invoiceId)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _salesInvoiceApiService.GetByIdAsync(invoiceId);
            if (res.Success && res.Data != null)
            {
                var invoice = res.Data;
                _selectedInvoiceItems = invoice.Items ?? new List<SalesInvoiceItemDto>();

                // المرتجع يعود حصراً لفرع ومستودع الفاتورة الأصلية
                SelectedBranch = Branches.FirstOrDefault(b => b.Id == invoice.BranchId);
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == invoice.WarehouseId);
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == invoice.CustomerId);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل الفاتورة";
            }
        });
    }

    public async Task InitializeAsync(Guid? id)
    {
        await LoadLookupDataAsync();

        if (id.HasValue)
        {
            IsViewOnly = true;
            ReturnId = id.Value;
            await LoadReturnDetailsAsync(id.Value);
        }
        else
        {
            IsViewOnly = false;
            ReturnId = null;
            ReturnNumber = $"RET-{DateTime.Now:yyyyMMddHHmmss}";
            ReturnDate = DateTime.Now;
            Reason = SalesReturnReason.CustomerChangedMind;
            Items.Clear();
            TotalAmount = 0;
            Notes = string.Empty;
            _selectedInvoiceItems = new List<SalesInvoiceItemDto>();
            SelectedInvoice = null;
            InvoiceSearchTerm = string.Empty;
            Invoices.Clear();
            IsInvoicelessMode = false;
        }
    }

    /// <summary>تهيئة النموذج محملاً بفاتورة أصلية محددة مسبقاً (من زر إنشاء مرتجع على بند فاتورة)</summary>
    public async Task InitializeForInvoiceAsync(Guid invoiceId, CreateSalesReturnItemRequest? prefilledItem = null)
    {
        await LoadLookupDataAsync();

        IsViewOnly = false;
        ReturnId = null;
        ReturnNumber = $"RET-{DateTime.Now:yyyyMMddHHmmss}";
        ReturnDate = DateTime.Now;
        Reason = SalesReturnReason.CustomerChangedMind;
        Items.Clear();
        TotalAmount = 0;
        Notes = string.Empty;
        _selectedInvoiceItems = new List<SalesInvoiceItemDto>();

        var res = await _salesInvoiceApiService.GetByIdAsync(invoiceId);
        if (res.Success && res.Data != null)
        {
            var invoice = res.Data;
            var summary = new SalesInvoiceSummaryDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                BranchId = invoice.BranchId,
                WarehouseId = invoice.WarehouseId,
                CustomerId = invoice.CustomerId,
                Status = invoice.Status
            };
            Invoices = new ObservableCollection<SalesInvoiceSummaryDto> { summary };

            _selectedInvoiceItems = invoice.Items ?? new List<SalesInvoiceItemDto>();
            SelectedBranch = Branches.FirstOrDefault(b => b.Id == invoice.BranchId);
            SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == invoice.WarehouseId);
            SelectedCustomer = Customers.FirstOrDefault(c => c.Id == invoice.CustomerId);
            SelectedInvoice = summary;
            InvoiceSearchTerm = invoice.InvoiceNumber;

            if (prefilledItem != null)
            {
                Items.Add(prefilledItem);
                TotalAmount = Items.Sum(i => i.LineTotal);
            }
        }
        else
        {
            ErrorMessage = res.Message ?? "فشل تحميل الفاتورة الأصلية";
        }
    }

    private async Task LoadLookupDataAsync()
    {
        var bRes = await _branchApiService.GetAllAsync();
        if (bRes.Success && bRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(bRes.Data);
            if (SelectedBranch == null && Branches.Count > 0) SelectedBranch = Branches[0];
        }

        var wRes = await _warehouseApiService.GetAllAsync();
        if (wRes.Success && wRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(wRes.Data);
            if (SelectedWarehouse == null && Warehouses.Count > 0) SelectedWarehouse = Warehouses[0];
        }

        var cRes = await _customerApiService.GetAllAsync(isActive: true);
        if (cRes.Success && cRes.Data != null)
        {
            Customers = new ObservableCollection<CustomerSummaryDto>(cRes.Data);
        }

        // الأصناف لا تُجلب كاملة؛ تُبحث تدريجياً بالاسم أو الباركود من الخادم
    }

    partial void OnProductSearchTermChanged(string? value)
    {
        if (IsViewOnly) return;
        // بحث مؤجل (Debounce) لتفادي إغراق الخادم بطلب لكل حرف
        _ = DebounceSearchAsync(SearchProductsAsync);
    }

    private async Task SearchProductsAsync()
    {
        if (IsViewOnly) return;

        var term = ProductSearchTerm?.Trim() ?? string.Empty;
        if (term.Length == 0)
        {
            AvailableProducts.Clear();
            return;
        }

        // مطابقة تامة: استعلام مباشر بنقطة الباركود الدقيقة
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
            // في وضع الفاتورة الأصلية تُقصر النتائج على أصناف بنود الفاتورة
            if (SelectedInvoice != null && !IsInvoicelessMode && _selectedInvoiceItems.Count > 0)
            {
                var invoiceProductIds = _selectedInvoiceItems.Select(i => i.ProductId).Distinct().ToHashSet();
                res.Data = res.Data.Where(p => invoiceProductIds.Contains(p.Id)).ToList();
            }
            AvailableProducts = new ObservableCollection<ProductDto>(res.Data);
        }
        else if (!res.Success)
        {
            ErrorMessage = res.Message;
        }
    }

    private async Task LoadReturnDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _salesReturnApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                var ret = res.Data;
                ReturnNumber = ret.ReturnNumber;
                ReturnDate = ret.ReturnDate;
                Reason = ret.Reason;
                Notes = ret.Notes;
                TotalAmount = ret.TotalAmount;

                SelectedBranch = Branches.FirstOrDefault(b => b.Id == ret.BranchId);
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == ret.WarehouseId);
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == ret.CustomerId);

                Items = new ObservableCollection<CreateSalesReturnItemRequest>(
                    ret.Items.Select(i => new CreateSalesReturnItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
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

    [RelayCommand]
    private void AddItem()
    {
        // في وضع الفاتورة الأصلية يُسمح فقط بالأصناف والباركودات الواردة في بنودها
        if (SelectedInvoice != null && !IsInvoicelessMode)
        {
            AddItemFromInvoice();
            return;
        }

        if (SelectedProductToAdd == null)
        {
            ErrorMessage = "يرجى اختيار صنف لإضافته";
            return;
        }

        if (QuantityToAdd <= 0)
        {
            ErrorMessage = "الكمية يجب أن تكون أكبر من صفر";
            return;
        }

        var item = new CreateSalesReturnItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            Quantity = QuantityToAdd,
            UnitPrice = UnitPriceToAdd
        };

        Items.Add(item);
        SelectedProductToAdd = null;
        QuantityToAdd = 1;
        UnitPriceToAdd = 0;
        ErrorMessage = null;

        TotalAmount = Items.Sum(i => i.LineTotal);
    }

    /// <summary>إضافة بند مرتجع من بنود الفاتورة الأصلية مع باركود البند والتحقق من سقف الكمية المباعة</summary>
    private void AddItemFromInvoice()
    {
        if (SelectedProductToAdd == null)
        {
            ErrorMessage = "يرجى اختيار صنف من بنود الفاتورة";
            return;
        }

        var invoiceItems = _selectedInvoiceItems
            .Where(i => i.ProductId == SelectedProductToAdd.Id)
            .ToList();

        if (invoiceItems.Count == 0)
        {
            ErrorMessage = "الصنف المحدد غير موجود ضمن بنود الفاتورة الأصلية";
            return;
        }

        if (QuantityToAdd <= 0)
        {
            ErrorMessage = "الكمية يجب أن تكون أكبر من صفر";
            return;
        }

        // مطابقة باركود بند الفاتورة (النكهة الأولى المطابقة للصنف)
        var invoiceItem = invoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue)
            ?? invoiceItems.First();

        var alreadyReturned = Items
            .Where(i => i.ProductId == SelectedProductToAdd.Id)
            .Sum(i => i.Quantity);
        var soldQuantity = invoiceItems.Sum(i => i.Quantity);

        if (alreadyReturned + QuantityToAdd > soldQuantity)
        {
            ErrorMessage = $"الكمية المرتجعة تتجاوز المباعة (المباعة: {soldQuantity} — المطلوب إرجاعه: {alreadyReturned + QuantityToAdd})";
            return;
        }

        var barcodeLabel = !string.IsNullOrWhiteSpace(invoiceItem.Barcode) ? $" ({invoiceItem.Barcode})" : string.Empty;
        var item = new CreateSalesReturnItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = $"{SelectedProductToAdd.Name}{barcodeLabel}",
            ProductBarCodeId = invoiceItem.ProductBarCodeId,
            Quantity = QuantityToAdd,
            UnitPrice = UnitPriceToAdd > 0 ? UnitPriceToAdd : invoiceItem.UnitPrice
        };

        Items.Add(item);
        SelectedProductToAdd = null;
        QuantityToAdd = 1;
        UnitPriceToAdd = 0;
        ErrorMessage = null;

        TotalAmount = Items.Sum(i => i.LineTotal);
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is CreateSalesReturnItemRequest item)
        {
            Items.Remove(item);
            TotalAmount = Items.Sum(i => i.LineTotal);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب اختيار الفرع";
            return;
        }

        if (SelectedInvoice == null && !IsInvoicelessMode && !CanReturnWithoutInvoice)
        {
            ErrorMessage = "يجب اختيار الفاتورة الأصلية — المرتجع بدون فاتورة يتطلب صلاحية خاصة";
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يجب اختيار صالة العرض أو المستودع المستلم";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل للمرتجع";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreateSalesReturnRequest
            {
                ReturnNumber = ReturnNumber,
                OriginalInvoiceId = SelectedInvoice?.Id,
                BranchId = SelectedBranch.Id,
                WarehouseId = SelectedWarehouse.Id,
                CustomerId = SelectedCustomer?.Id,
                ReturnDate = ReturnDate,
                Reason = Reason,
                Notes = Notes,
                Items = Items.ToList()
            };
            var res = await _salesReturnApiService.CreateAsync(req);
            if (res.Success) CloseWindowHandler?.Invoke();
            else ErrorMessage = res.Message ?? "فشل تسجيل المرتجع";
        });
    }
}
