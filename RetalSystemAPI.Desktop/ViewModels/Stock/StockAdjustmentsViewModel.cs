using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Stock;

public partial class StockAdjustmentsViewModel : BaseViewModel
{
    private int _loadVersion;

    private readonly IStockAdjustmentApiService _stockAdjustmentApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<StockAdjustmentSummaryDto> _adjustments = new();

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private StockAdjustmentReason? _selectedReason;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<StockAdjustmentSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public StockAdjustmentsViewModel(
        IStockAdjustmentApiService stockAdjustmentApiService,
        IWarehouseApiService warehouseApiService)
    {
        _stockAdjustmentApiService = stockAdjustmentApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadWarehousesAsync();
        await LoadAdjustmentsAsync();
    }

    private async Task LoadWarehousesAsync()
    {
        var whRes = await _warehouseApiService.GetAllAsync();
        if (whRes.Success && whRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
        }
    }

    [RelayCommand]
    public async Task LoadAdjustmentsAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            var res = await _stockAdjustmentApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                warehouseId: SelectedWarehouse?.Id,
                reason: SelectedReason,
                search: SearchQuery);
            if (version != _loadVersion) return;

            if (res.Success && res.Data != null)
            {
                Adjustments = new ObservableCollection<StockAdjustmentSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadAdjustmentsAsync();
                    return;
                }
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل التسويات الجردية";
            }
        }, isCurrent: () => version == _loadVersion);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadAdjustmentsAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadAdjustmentsAsync();
        }
    }

    [RelayCommand]
    private async Task CreateAdjustmentAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadAdjustmentsAsync();
        }
    }

    [RelayCommand]
    private async Task ViewAdjustmentDetailsAsync(object? parameter)
    {
        if (parameter is StockAdjustmentSummaryDto adj && OpenDialogHandler != null)
        {
            await OpenDialogHandler(adj);
        }
    }

    [RelayCommand]
    private async Task DeleteAdjustmentAsync(object? parameter)
    {
        if (parameter is not StockAdjustmentSummaryDto adj) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف التسوية", $"هل أنت متأكد من حذف التسوية الجردية رقم '{adj.AdjustmentNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _stockAdjustmentApiService.DeleteAsync(adj.Id);
            if (res.Success) await LoadAdjustmentsAsync();
            else ErrorMessage = res.Message;
        });
    }
    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadAdjustmentsAsync();
    }
    partial void OnSelectedReasonChanged(StockAdjustmentReason? value)
    {
        CurrentPage = 1;
        _ = LoadAdjustmentsAsync();
    }
    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadAdjustmentsAsync);
    }
    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadAdjustmentsAsync();
    }
}

public partial class StockAdjustmentFormViewModel : BaseViewModel
{
    private readonly IStockAdjustmentApiService _stockAdjustmentApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductApiService _productApiService;
    private readonly IStockApiService _stockApiService;

    [ObservableProperty]
    private Guid? _adjustmentId;

    [ObservableProperty]
    private string _adjustmentNumber = string.Empty;

    [ObservableProperty]
    private DateTime _adjustmentDate = DateTime.Now;

    [ObservableProperty]
    private StockAdjustmentReason _reason = StockAdjustmentReason.InventoryCount;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        IsStorageWarehouse = (value?.Type == WarehouseType.Storge);
        _ = UpdateSystemQuantityAsync();
    }

    [ObservableProperty]
    private bool _isStorageWarehouse;

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

    partial void OnProductSearchTermChanged(string value)
    {
        if (IsViewOnly) return;
        // بحث مؤجل (Debounce) لتفادي إغراق الخادم بطلب لكل حرف
        _ = DebounceSearchAsync(SearchProductsAsync);
    }

    /// <summary>بحث الأصناف من الخادم بالاسم أو الباركود — أو مطابقة تامة عند التفعيل</summary>
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
                SelectedProductToAdd = exactRes.Data;
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

    partial void OnSelectedProductToAddChanged(ProductDto? value)
    {
        AvailableBarcodes.Clear();
        if (value != null)
        {
            UnitCostToAdd = value.CostPrice;
            if (value.BarCodes != null && value.BarCodes.Count > 0)
            {
                foreach (var bc in value.BarCodes)
                {
                    AvailableBarcodes.Add(bc);
                }
                SelectedBarcodeToAdd = AvailableBarcodes.FirstOrDefault();
            }
            else
            {
                SelectedBarcodeToAdd = null;
            }
        }
        else
        {
            SelectedBarcodeToAdd = null;
            UnitCostToAdd = 0;
        }

        _ = UpdateSystemQuantityAsync();
    }

    [ObservableProperty]
    private ObservableCollection<ProductBarCodeDto> _availableBarcodes = new();

    [ObservableProperty]
    private ProductBarCodeDto? _selectedBarcodeToAdd;

    partial void OnSelectedBarcodeToAddChanged(ProductBarCodeDto? value)
    {
        _ = UpdateSystemQuantityAsync();
    }

    [ObservableProperty]
    private int _systemQuantityToAdd;

    [ObservableProperty]
    private int _actualQuantityToAdd = 1;

    [ObservableProperty]
    private decimal _unitCostToAdd;

    [ObservableProperty]
    private StockAdjustmentReason _reasonToAdd = StockAdjustmentReason.InventoryCount;

    [ObservableProperty]
    private ObservableCollection<CreateStockAdjustmentItemRequest> _items = new();

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isViewOnly;

    public Action? CloseWindowHandler { get; set; }

    public StockAdjustmentFormViewModel(
        IStockAdjustmentApiService stockAdjustmentApiService,
        IWarehouseApiService warehouseApiService,
        IProductApiService productApiService,
        IStockApiService stockApiService)
    {
        _stockAdjustmentApiService = stockAdjustmentApiService;
        _warehouseApiService = warehouseApiService;
        _productApiService = productApiService;
        _stockApiService = stockApiService;
    }

    private async Task UpdateSystemQuantityAsync()
    {
        if (SelectedWarehouse == null || SelectedProductToAdd == null)
        {
            SystemQuantityToAdd = 0;
            return;
        }

        try
        {
            if (SelectedWarehouse.Type == WarehouseType.Storge)
            {
                if (SelectedBarcodeToAdd != null)
                {
                    var res = await _stockApiService.GetStorgeStockAsync(SelectedWarehouse.Id, SelectedBarcodeToAdd.Id);
                    SystemQuantityToAdd = (res.Success && res.Data != null) ? (int)res.Data.Quantity : 0;
                }
                else
                {
                    SystemQuantityToAdd = 0;
                }
            }
            else if (SelectedWarehouse.Type == WarehouseType.Show)
            {
                var res = await _stockApiService.GetShowroomStockAsync(SelectedWarehouse.Id, SelectedProductToAdd.Id);
                SystemQuantityToAdd = (res.Success && res.Data != null) ? (int)res.Data.Quantity : 0;
            }
        }
        catch
        {
            SystemQuantityToAdd = 0;
        }
    }

    public async Task InitializeAsync(Guid? id)
    {
        await LoadLookupDataAsync();

        if (id.HasValue)
        {
            IsViewOnly = true;
            AdjustmentId = id.Value;
            await LoadAdjustmentDetailsAsync(id.Value);
        }
        else
        {
            IsViewOnly = false;
            AdjustmentId = null;
            AdjustmentNumber = $"ADJ-{DateTime.Now:yyyyMMddHHmmss}";
            AdjustmentDate = DateTime.Now;
            Reason = StockAdjustmentReason.InventoryCount;
            Items.Clear();
            Notes = string.Empty;
        }
    }

    private async Task LoadLookupDataAsync()
    {
        var wRes = await _warehouseApiService.GetAllAsync();
        if (wRes.Success && wRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(wRes.Data);
            if (SelectedWarehouse == null && Warehouses.Count > 0) SelectedWarehouse = Warehouses[0];
            IsStorageWarehouse = (SelectedWarehouse?.Type == WarehouseType.Storge);
        }

        // الأصناف لا تُجلب كاملة؛ تُبحث تدريجياً بالاسم أو الباركود من الخادم
    }

    private async Task LoadAdjustmentDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _stockAdjustmentApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                var adj = res.Data;
                AdjustmentNumber = adj.AdjustmentNumber;
                AdjustmentDate = adj.AdjustmentDate;
                Reason = adj.Reason;
                Notes = adj.Notes;

                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == adj.WarehouseId);
                IsStorageWarehouse = (SelectedWarehouse?.Type == WarehouseType.Storge);
                Items = new ObservableCollection<CreateStockAdjustmentItemRequest>(
                    adj.Items.Select(i => new CreateStockAdjustmentItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductBarCodeId = i.ProductBarCodeId,
                        BarcodeTitle = i.BarcodeTitle,
                        BarcodeValue = i.BarcodeValue,
                        SystemQuantity = i.SystemQuantity,
                        ActualQuantity = i.ActualQuantity,
                        UnitCost = i.UnitCost,
                        // سبب البند الفعلي المحفوظ لكل بند على حدة
                        Reason = i.Reason
                    }));
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل التسوية الجردية";
            }
        });
    }

    [RelayCommand]
    private void AddItem()
    {
        if (SelectedProductToAdd == null)
        {
            ErrorMessage = "يرجى اختيار صنف لإضافته";
            return;
        }

        Guid? barcodeId = null;
        string? barcodeTitle = null;
        string? barcodeVal = null;
        string displayName = SelectedProductToAdd.Name;

        if (IsStorageWarehouse)
        {
            if (SelectedBarcodeToAdd == null && AvailableBarcodes.Count > 0)
            {
                ErrorMessage = "يرجى اختيار النكهة / الباركود المراد جرده";
                return;
            }

            if (SelectedBarcodeToAdd != null)
            {
                barcodeId = SelectedBarcodeToAdd.Id;
                barcodeTitle = !string.IsNullOrWhiteSpace(SelectedBarcodeToAdd.Title) ? SelectedBarcodeToAdd.Title : SelectedBarcodeToAdd.Description;
                barcodeVal = SelectedBarcodeToAdd.BarCode;
                displayName = !string.IsNullOrWhiteSpace(barcodeTitle)
                    ? $"{SelectedProductToAdd.Name} - {barcodeTitle}"
                    : $"{SelectedProductToAdd.Name} ({barcodeVal})";
            }
        }

        var item = new CreateStockAdjustmentItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = displayName,
            ProductBarCodeId = barcodeId,
            BarcodeTitle = barcodeTitle,
            BarcodeValue = barcodeVal,
            SystemQuantity = SystemQuantityToAdd,
            ActualQuantity = ActualQuantityToAdd,
            UnitCost = UnitCostToAdd,
            Reason = ReasonToAdd
        };

        Items.Add(item);

        // مزامنة سبب التسوية العام مع سبب البند إن كان لا يزال على الافتراضي (جرد دوري)،
        // حتى لا تُعرض التسوية في القوائم بسبب عام لا يعكس ما أدخله المستخدم
        if (Reason == StockAdjustmentReason.InventoryCount && ReasonToAdd != StockAdjustmentReason.InventoryCount)
        {
            Reason = ReasonToAdd;
        }

        ErrorMessage = string.Empty;
        SelectedProductToAdd = null;
        SelectedBarcodeToAdd = null;
        SystemQuantityToAdd = 0;
        ActualQuantityToAdd = 1;
        UnitCostToAdd = 0;
        ReasonToAdd = StockAdjustmentReason.InventoryCount;
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is CreateStockAdjustmentItemRequest item)
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يجب اختيار المستودع أو صالة العرض المعنية بالجرد";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل للتسوية الجردية";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreateStockAdjustmentRequest
            {
                AdjustmentNumber = AdjustmentNumber,
                WarehouseId = SelectedWarehouse.Id,
                AdjustmentDate = AdjustmentDate,
                Reason = Reason,
                Notes = Notes,
                Items = Items.ToList()
            };
            var res = await _stockAdjustmentApiService.CreateAsync(req);
            if (res.Success) CloseWindowHandler?.Invoke();
            else ErrorMessage = res.Message ?? "فشل تسجيل التسوية الجردية";
        });
    }
}
