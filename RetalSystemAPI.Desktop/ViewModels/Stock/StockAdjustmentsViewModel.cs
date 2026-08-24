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
        await ExecuteAsync(async () =>
        {
            var res = await _stockAdjustmentApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                warehouseId: SelectedWarehouse?.Id,
                reason: SelectedReason,
                search: SearchQuery);

            if (res.Success && res.Data != null)
            {
                Adjustments = new ObservableCollection<StockAdjustmentSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل التسويات الجردية";
            }
        });
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
}

public partial class StockAdjustmentFormViewModel : BaseViewModel
{
    private readonly IStockAdjustmentApiService _stockAdjustmentApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductApiService _productApiService;

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

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedProductToAdd;

    [ObservableProperty]
    private int _systemQuantityToAdd;

    [ObservableProperty]
    private int _actualQuantityToAdd = 1;

    [ObservableProperty]
    private decimal _unitCostToAdd;

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
        IProductApiService productApiService)
    {
        _stockAdjustmentApiService = stockAdjustmentApiService;
        _warehouseApiService = warehouseApiService;
        _productApiService = productApiService;
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
        }

        var pRes = await _productApiService.GetAllAsync();
        if (pRes.Success && pRes.Data != null)
        {
            AvailableProducts = new ObservableCollection<ProductDto>(pRes.Data);
        }
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

                Items = new ObservableCollection<CreateStockAdjustmentItemRequest>(
                    adj.Items.Select(i => new CreateStockAdjustmentItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        SystemQuantity = i.SystemQuantity,
                        ActualQuantity = i.ActualQuantity,
                        UnitCost = i.UnitCost
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

        var item = new CreateStockAdjustmentItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            SystemQuantity = SystemQuantityToAdd,
            ActualQuantity = ActualQuantityToAdd,
            UnitCost = UnitCostToAdd
        };

        Items.Add(item);
        SelectedProductToAdd = null;
        SystemQuantityToAdd = 0;
        ActualQuantityToAdd = 1;
        UnitCostToAdd = 0;
        ErrorMessage = null;
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
