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
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Purchase;

public partial class PurchaseOrdersViewModel : BaseViewModel
{
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
        _ = LoadOrdersAsync();
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
        await ExecuteAsync(async () =>
        {
            Guid? bId = SelectedBranch?.Id;
            Guid? wId = SelectedWarehouse?.Id;
            var response = await _purchaseOrderApiService.GetPagedAsync(CurrentPage, PageSize, bId, wId, SelectedStatus, SearchQuery);
            if (response.Success && response.Data != null)
            {
                Orders = new ObservableCollection<PurchaseOrderSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else ErrorMessage = response.Message ?? "فشل تحميل الطلبيات والمشتريات";
        });
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
        // تدوير الحالة
        PurchaseOrderStatus nextStatus = order.Status switch
        {
            PurchaseOrderStatus.Draft => PurchaseOrderStatus.Submitted,
            PurchaseOrderStatus.Submitted => PurchaseOrderStatus.Received,
            PurchaseOrderStatus.Received => PurchaseOrderStatus.Canceled,
            _ => PurchaseOrderStatus.Draft
        };

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

public partial class PurchaseOrderFormViewModel : BaseViewModel
{
    private readonly IPurchaseOrderApiService _purchaseOrderApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductBarCodeApiService _barCodeApiService;

    [ObservableProperty]
    private Guid? _orderId;

    [ObservableProperty]
    private string _orderNumber = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

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

    public Action? CloseWindowHandler { get; set; }

    public PurchaseOrderFormViewModel(
        IPurchaseOrderApiService purchaseOrderApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        IProductBarCodeApiService barCodeApiService)
    {
        _purchaseOrderApiService = purchaseOrderApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _barCodeApiService = barCodeApiService;
    }

    public async Task InitializeAsync(Guid? id)
    {
        var bRes = await _branchApiService.GetAllAsync();
        if (bRes.Success && bRes.Data != null) Branches = new ObservableCollection<BranchDto>(bRes.Data);

        var wRes = await _warehouseApiService.GetAllAsync();
        if (wRes.Success && wRes.Data != null) Warehouses = new ObservableCollection<WarehouseSummaryDto>(wRes.Data);

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
            SelectedWarehouse = null;
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
    private void AddItem(PurchaseOrderItemDto item)
    {
        Items.Add(item);
        RecalculateTotal();
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
