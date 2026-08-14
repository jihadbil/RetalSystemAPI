using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Stock;

public partial class StockViewModel : BaseViewModel
{
    private readonly IStockApiService _stockApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private ObservableCollection<StorgeStockDto> _storgeStocks = new();

    [ObservableProperty]
    private ObservableCollection<ShowroomStockDto> _showroomStocks = new();

    [ObservableProperty]
    private ObservableCollection<StorgeStockDto> _lowStorgeAlerts = new();

    [ObservableProperty]
    private ObservableCollection<ShowroomStockDto> _lowShowroomAlerts = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0; // 0: Storge, 1: Showroom, 2: Low Stock Alerts

    [ObservableProperty] private int _pageNumber = 1;
    [ObservableProperty] private int _pageSize = 10;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _totalCount = 0;
    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private bool _hasPreviousPage = false;
    [ObservableProperty] private bool _hasNextPage = false;

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        PageNumber = 1;
        _ = RefreshCurrentTabAsync();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        PageNumber = 1;
        _ = RefreshCurrentTabAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        PageNumber = 1;
        _ = RefreshCurrentTabAsync();
    }

    public Func<WarehouseSummaryDto?, object?, Task>? OpenSetStockDialogHandler { get; set; }

    public StockViewModel(IStockApiService stockApiService, IWarehouseApiService warehouseApiService)
    {
        _stockApiService = stockApiService;
        _warehouseApiService = warehouseApiService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadWarehousesAsync();
        await RefreshCurrentTabAsync();
    }

    private async Task LoadWarehousesAsync()
    {
        var res = await _warehouseApiService.GetAllAsync();
        if (res.Success && res.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(res.Data);
            if (Warehouses.Count > 0)
            {
                SelectedWarehouse = Warehouses[0];
            }
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        PageNumber = 1;
        await RefreshCurrentTabAsync();
    }

    [RelayCommand]
    public async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            PageNumber++;
            await RefreshCurrentTabAsync();
        }
    }

    [RelayCommand]
    public async Task PreviousPageAsync()
    {
        if (HasPreviousPage && PageNumber > 1)
        {
            PageNumber--;
            await RefreshCurrentTabAsync();
        }
    }

    [RelayCommand]
    public async Task RefreshCurrentTabAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (SelectedTabIndex == 0)
            {
                if (SelectedWarehouse != null)
                {
                    var res = await _stockApiService.GetPagedStorgeStocksByWarehouseAsync(SelectedWarehouse.Id, PageNumber, PageSize, SearchTerm);
                    if (res.Success && res.Data != null)
                    {
                        StorgeStocks = new ObservableCollection<StorgeStockDto>(res.Data.Items);
                        PageNumber = res.Data.PageNumber;
                        TotalPages = res.Data.TotalPages;
                        TotalCount = res.Data.TotalCount;
                        HasPreviousPage = res.Data.HasPreviousPage;
                        HasNextPage = res.Data.HasNextPage;
                    }
                }
            }
            else if (SelectedTabIndex == 1)
            {
                if (SelectedWarehouse != null)
                {
                    var res = await _stockApiService.GetPagedShowroomStocksByWarehouseAsync(SelectedWarehouse.Id, PageNumber, PageSize, SearchTerm);
                    if (res.Success && res.Data != null)
                    {
                        ShowroomStocks = new ObservableCollection<ShowroomStockDto>(res.Data.Items);
                        PageNumber = res.Data.PageNumber;
                        TotalPages = res.Data.TotalPages;
                        TotalCount = res.Data.TotalCount;
                        HasPreviousPage = res.Data.HasPreviousPage;
                        HasNextPage = res.Data.HasNextPage;
                    }
                }
            }
            else if (SelectedTabIndex == 2)
            {
                Guid? warehouseId = SelectedWarehouse?.Id;
                var res1 = await _stockApiService.GetLowStorgeStockAlertsAsync(warehouseId);
                if (res1.Success && res1.Data != null)
                    LowStorgeAlerts = new ObservableCollection<StorgeStockDto>(res1.Data);

                var res2 = await _stockApiService.GetLowShowroomStockAlertsAsync(warehouseId);
                if (res2.Success && res2.Data != null)
                    LowShowroomAlerts = new ObservableCollection<ShowroomStockDto>(res2.Data);
            }
        });
    }

    [RelayCommand]
    private async Task EditStorgeStockAsync(object? parameter)
    {
        if (parameter is StorgeStockDto stock && SelectedWarehouse != null && OpenSetStockDialogHandler != null)
        {
            await OpenSetStockDialogHandler(SelectedWarehouse, stock);
            await RefreshCurrentTabAsync();
        }
    }

    [RelayCommand]
    private async Task EditShowroomStockAsync(object? parameter)
    {
        if (parameter is ShowroomStockDto stock && SelectedWarehouse != null && OpenSetStockDialogHandler != null)
        {
            await OpenSetStockDialogHandler(SelectedWarehouse, stock);
            await RefreshCurrentTabAsync();
        }
    }
}

public partial class SetStockFormViewModel : BaseViewModel
{
    private readonly IStockApiService _stockApiService;

    [ObservableProperty]
    private WarehouseSummaryDto? _warehouse;

    [ObservableProperty]
    private Guid _targetId; // ProductBarcodeId or ProductId

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private decimal _quantity;

    [ObservableProperty]
    private int _minStockLevel;

    [ObservableProperty]
    private bool _isStorgeMode;

    public Action? CloseWindowHandler { get; set; }

    public SetStockFormViewModel(IStockApiService stockApiService)
    {
        _stockApiService = stockApiService;
    }

    public void Initialize(WarehouseSummaryDto warehouse, object? stockItem)
    {
        Warehouse = warehouse;
        if (stockItem is StorgeStockDto storgeItem)
        {
            IsStorgeMode = true;
            TargetId = storgeItem.ProductBarcodeId;
            Title = $"{storgeItem.ProductName} - ({storgeItem.BarcodeTitle})";
            Quantity = storgeItem.Quantity;
            MinStockLevel = storgeItem.MinStockLevel;
        }
        else if (stockItem is ShowroomStockDto showroomItem)
        {
            IsStorgeMode = false;
            TargetId = showroomItem.ProductId;
            Title = showroomItem.ProductName;
            Quantity = showroomItem.Quantity;
            MinStockLevel = showroomItem.MinStockLevel;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Warehouse == null) return;
        await ExecuteAsync(async () =>
        {
            if (IsStorgeMode)
            {
                var req = new SetStorgeStockRequest
                {
                    WarehouseId = Warehouse.Id,
                    ProductBarcodeId = TargetId,
                    Quantity = Quantity,
                    MinStockLevel = MinStockLevel
                };
                var res = await _stockApiService.SetStorgeStockAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new SetShowroomStockRequest
                {
                    WarehouseId = Warehouse.Id,
                    ProductId = TargetId,
                    Quantity = Quantity,
                    MinStockLevel = MinStockLevel
                };
                var res = await _stockApiService.SetShowroomStockAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
        });
    }
}
