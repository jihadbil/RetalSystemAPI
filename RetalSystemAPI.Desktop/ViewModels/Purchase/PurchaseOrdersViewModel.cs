using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Purchase;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Purchase;

public partial class PurchaseOrdersViewModel : BaseViewModel
{
    private int _loadVersion;

    private readonly IPurchaseOrderApiService _purchaseOrderApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderSummaryDto> _orders = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private PurchaseOrderStatus? _selectedStatus;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnSelectedStatusChanged(PurchaseOrderStatus? value)
    {
        CurrentPage = 1;
        _ = LoadOrdersAsync();
    }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadOrdersAsync);
    }

    public Func<PurchaseOrderSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public PurchaseOrdersViewModel(
        IPurchaseOrderApiService purchaseOrderApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService)
    {
        _purchaseOrderApiService = purchaseOrderApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAndWarehousesAsync();
        await LoadOrdersAsync();
    }

    private async Task LoadBranchesAndWarehousesAsync()
    {
        var bRes = await _branchApiService.GetAllAsync();
        if (bRes.Success && bRes.Data != null) Branches = new ObservableCollection<BranchDto>(bRes.Data);

        var wRes = await _warehouseApiService.GetAllAsync();
        if (wRes.Success && wRes.Data != null) Warehouses = new ObservableCollection<WarehouseSummaryDto>(wRes.Data);
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            Guid? bId = SelectedBranch?.Id;
            Guid? wId = SelectedWarehouse?.Id;
            var response = await _purchaseOrderApiService.GetPagedAsync(CurrentPage, PageSize, bId, wId, SelectedStatus, SearchQuery);
            if (version != _loadVersion) return;
            if (response.Success && response.Data != null)
            {
                Orders = new ObservableCollection<PurchaseOrderSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadOrdersAsync();
                    return;
                }
            }
            else ErrorMessage = response.Message ?? "فشل تحميل الطلبيات والمشتريات";
        }, isCurrent: () => version == _loadVersion);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages) { CurrentPage++; await LoadOrdersAsync(); }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1) { CurrentPage--; await LoadOrdersAsync(); }
    }

    [RelayCommand]
    private async Task CreateOrderAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task EditOrderAsync(object? parameter)
    {
        if (parameter is PurchaseOrderSummaryDto order && OpenDialogHandler != null)
        {
            await OpenDialogHandler(order);
            await LoadOrdersAsync();
        }
    }

    [RelayCommand]
    private async Task UpdateStatusAsync(object? parameter)
    {
        if (parameter is not PurchaseOrderSummaryDto order) return;

        if (order.Status == PurchaseOrderStatus.Received)
        {
            ErrorMessage = "الطلبية مستلمة ومغلقة بالفعل؛ تم إيداع الأصناف في المخزون لمرة واحدة ولا يمكن تكرار الاستلام.";
            return;
        }

        if (order.Status == PurchaseOrderStatus.Cancelled)
        {
            ErrorMessage = "الطلبية ملغاة ولا يمكن تغيير حالتها.";
            return;
        }

        PurchaseOrderStatus nextStatus;
        string confirmMsg;
        if (order.Status == PurchaseOrderStatus.Draft)
        {
            nextStatus = PurchaseOrderStatus.Confirmed;
            confirmMsg = $"هل تريد تأكيد واعتماد الطلبية '{order.OrderNumber}'؟";
        }
        else if (order.Status == PurchaseOrderStatus.Confirmed)
        {
            nextStatus = PurchaseOrderStatus.Received;
            confirmMsg = $"هل تريد استلام بضاعة الطلبية '{order.OrderNumber}' وإيداعها في المخزون؟ (سيتم ترحيل الكميات لمرة واحدة فقط وإغلاق الطلبية نهائياً)";
        }
        else
        {
            return;
        }

        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد تحديث حالة الطلبية", confirmMsg);
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _purchaseOrderApiService.UpdateStatusAsync(order.Id, nextStatus);
            if (res.Success) await LoadOrdersAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task DeleteOrderAsync(object? parameter)
    {
        if (parameter is not PurchaseOrderSummaryDto order) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف الطلبية", $"هل أنت تأكد من حذف الطلبية '{order.OrderNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _purchaseOrderApiService.DeleteAsync(order.Id);
            if (res.Success) await LoadOrdersAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public class BarcodeOptionItem
{
    public Guid BarCodeId { get; set; }
    public string BarcodeValue { get; set; } = string.Empty;
    public string BarcodeTitle { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public string DisplayText => $"[{BarcodeValue}] {ProductName} - {BarcodeTitle} ({CostPrice:N2} د.ل)";
}

public partial class PurchaseOrderFormViewModel : BaseViewModel
{
    private readonly IPurchaseOrderApiService _purchaseOrderApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductBarCodeApiService _barCodeApiService;
    private readonly ISupplierApiService _supplierApiService;

    [ObservableProperty]
    private Guid? _orderId;

    [ObservableProperty]
    private string _orderNumber = string.Empty;

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
    private string _itemSearchQuery = string.Empty;

    /// <summary>مطابقة الباركود تماماً — يبحث بالتطابق التام مع رقم الباركود</summary>
    [ObservableProperty]
    private bool _exactBarcodeMatch = false;

    [ObservableProperty]
    private ObservableCollection<BarcodeOptionItem> _barcodeOptions = new();

    [ObservableProperty]
    private ObservableCollection<BarcodeOptionItem> _filteredBarcodeOptions = new();

    [ObservableProperty]
    private BarcodeOptionItem? _selectedBarcodeOption;

    [ObservableProperty]
    private decimal _inputQuantity = 1;

    [ObservableProperty]
    private decimal _inputUnitPrice = 0;

    [ObservableProperty]
    private DateTime _orderDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? _expectedDate;

    [ObservableProperty]
    private ObservableCollection<PurchaseOrderItemDto> _items = new();

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private bool _isEditMode;

    private int _barcodeSearchVersion;

    partial void OnItemSearchQueryChanged(string value)
    {
        // بحث مؤجل (Debounce) في باركودات الخادم بدل تحميلها كاملة
        _ = DebounceSearchAsync(SearchBarcodeOptionsAsync);
    }

    partial void OnSelectedBarcodeOptionChanged(BarcodeOptionItem? value)
    {
        if (value != null)
        {
            InputUnitPrice = value.CostPrice;
            if (InputQuantity <= 0) InputQuantity = 1;
        }
    }

    /// <summary>بحث باركودات الأصناف من الخادم بالاسم أو الباركود — أو مطابقة تامة عند التفعيل</summary>
    private async Task SearchBarcodeOptionsAsync()
    {
        var version = ++_barcodeSearchVersion;
        var q = ItemSearchQuery?.Trim() ?? string.Empty;

        if (q.Length == 0)
        {
            if (version != _barcodeSearchVersion) return;
            FilteredBarcodeOptions.Clear();
            SelectedBarcodeOption = null;
            return;
        }

        // مطابقة تامة: استعلام مباشر بنقطة الباركود الدقيقة ثم جلب كل باركودات الصنف المطابق
        List<BarcodeOptionItem> options;
        if (ExactBarcodeMatch)
        {
            options = new List<BarcodeOptionItem>();
            // البحث الشامل بالباركود يعيد باركودات المنتج الذي يحتوي القيمة؛ نصفّي للتطابق الحرفي التام
            var res = await _barCodeApiService.GetAllAsync(q);
            if (version != _barcodeSearchVersion) return;
            if (res.Success && res.Data != null)
            {
                options = res.Data
                    .Where(bc => string.Equals(bc.BarCode, q, StringComparison.OrdinalIgnoreCase))
                    .Select(bc => new BarcodeOptionItem
                    {
                        BarCodeId = bc.Id,
                        BarcodeValue = bc.BarCode,
                        BarcodeTitle = bc.Title,
                        ProductName = bc.ProductName,
                        CostPrice = bc.CostPrice
                    }).ToList();
            }
        }
        else
        {
            var res = await _barCodeApiService.GetAllAsync(q);
            if (version != _barcodeSearchVersion) return;
            options = (res.Success && res.Data != null)
                ? res.Data.Select(bc => new BarcodeOptionItem
                {
                    BarCodeId = bc.Id,
                    BarcodeValue = bc.BarCode,
                    BarcodeTitle = bc.Title,
                    ProductName = bc.ProductName,
                    CostPrice = bc.CostPrice
                }).ToList()
                : new List<BarcodeOptionItem>();
        }

        if (version != _barcodeSearchVersion) return;
        BarcodeOptions = new ObservableCollection<BarcodeOptionItem>(options);
        FilteredBarcodeOptions = new ObservableCollection<BarcodeOptionItem>(options);
        SelectedBarcodeOption = FilteredBarcodeOptions.FirstOrDefault();
    }

    private void FilterBarcodeOptions()
    {
        // النسخة الكاملة تُدار الآن عبر البحث الخادمي — تبقى للتوافق مع الاستدعاءات القديمة
        FilteredBarcodeOptions = new ObservableCollection<BarcodeOptionItem>(BarcodeOptions);
        if (FilteredBarcodeOptions.Count > 0)
        {
            SelectedBarcodeOption = FilteredBarcodeOptions[0];
        }
        else
        {
            SelectedBarcodeOption = null;
        }
    }

    public Action? CloseWindowHandler { get; set; }

    public PurchaseOrderFormViewModel(
        IPurchaseOrderApiService purchaseOrderApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        IProductBarCodeApiService barCodeApiService,
        ISupplierApiService supplierApiService)
    {
        _purchaseOrderApiService = purchaseOrderApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _barCodeApiService = barCodeApiService;
        _supplierApiService = supplierApiService;
    }

    public async Task InitializeAsync(Guid? id)
    {
        var bRes = await _branchApiService.GetAllAsync();
        if (bRes.Success && bRes.Data != null) Branches = new ObservableCollection<BranchDto>(bRes.Data);

        var sRes = await _supplierApiService.GetAllAsync();
        if (sRes.Success && sRes.Data != null) Suppliers = new ObservableCollection<SupplierSummaryDto>(sRes.Data);

        var wRes = await _warehouseApiService.GetAllAsync();
        if (wRes.Success && wRes.Data != null) Warehouses = new ObservableCollection<WarehouseSummaryDto>(wRes.Data);

        // الباركودات لا تُجلب كاملة؛ تُبحث تدريجياً بالاسم أو الباركود من الخادم
        BarcodeOptions.Clear();
        FilteredBarcodeOptions.Clear();

        if (id.HasValue)
        {
            IsEditMode = true;
            OrderId = id.Value;
            await LoadOrderDetailsAsync(id.Value);
        }
        else
        {
            IsEditMode = false;
            OrderId = null;
            OrderNumber = $"PO-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            SelectedBranch = Branches.Count > 0 ? Branches[0] : null;
            SelectedSupplier = Suppliers.Count > 0 ? Suppliers[0] : null;
            SelectedWarehouse = Warehouses.Count > 0 ? Warehouses[0] : null;
            OrderDate = DateTime.Now;
            ExpectedDate = DateTime.Now.AddDays(7);
            Items = new ObservableCollection<PurchaseOrderItemDto>();
            RecalculateTotal();
        }
    }

    private async Task LoadOrderDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _purchaseOrderApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                OrderNumber = res.Data.OrderNumber;
                SelectedBranch = Branches.FirstOrDefault(b => b.Id == res.Data.BranchId);
                SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == res.Data.SupplierId);
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == res.Data.WarehouseId);
                OrderDate = res.Data.OrderDate;
                ExpectedDate = res.Data.ExpectedDate;
                Items = new ObservableCollection<PurchaseOrderItemDto>(res.Data.Items);
                RecalculateTotal();
            }
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedBarcodeOption == null)
        {
            ErrorMessage = "يرجى كتابة اسم المنتج أو الباركود واختيار الصنف المطابق أولاً";
            return;
        }

        if (InputQuantity <= 0)
        {
            ErrorMessage = "يرجى إدخال كمية أكبر من صفر";
            return;
        }

        var item = new PurchaseOrderItemDto
        {
            ProductBarCodeId = SelectedBarcodeOption.BarCodeId,
            BarcodeValue = SelectedBarcodeOption.BarcodeValue,
            BarcodeTitle = SelectedBarcodeOption.BarcodeTitle,
            ProductName = SelectedBarcodeOption.ProductName,
            Quantity = InputQuantity,
            UnitPrice = InputUnitPrice,
            LineTotal = InputQuantity * InputUnitPrice
        };

        Items.Add(item);
        RecalculateTotal();
        ErrorMessage = string.Empty;

        ItemSearchQuery = string.Empty;
        SelectedBarcodeOption = null;
        InputQuantity = 1;
        InputUnitPrice = 0;
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is PurchaseOrderItemDto item)
        {
            Items.Remove(item);
            RecalculateTotal();
        }
    }

    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Quantity * i.UnitPrice);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(OrderNumber))
        {
            ErrorMessage = "رقم الطلبية مطلوب";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "اختيار الفرع مطلوب";
            return;
        }

        if (Items.Count == 0)
        {
            ErrorMessage = "يرجى إضافة بند واحد على الأقل للطلبية";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var itemRequests = Items.Select(i => new CreatePurchaseOrderItemRequest
            {
                ProductBarCodeId = i.ProductBarCodeId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            if (IsEditMode && OrderId.HasValue)
            {
                var req = new UpdatePurchaseOrderRequest
                {
                    SupplierId = SelectedSupplier?.Id,
                    WarehouseId = SelectedWarehouse?.Id,
                    ExpectedDate = ExpectedDate,
                    Items = itemRequests
                };
                var res = await _purchaseOrderApiService.UpdateAsync(OrderId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new CreatePurchaseOrderRequest
                {
                    OrderNumber = OrderNumber,
                    SupplierId = SelectedSupplier?.Id,
                    BranchId = SelectedBranch.Id,
                    WarehouseId = SelectedWarehouse?.Id,
                    OrderDate = OrderDate,
                    ExpectedDate = ExpectedDate,
                    Items = itemRequests
                };
                var res = await _purchaseOrderApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
        });
    }
}
