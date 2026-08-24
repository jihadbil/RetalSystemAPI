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

public partial class StockTransfersViewModel : BaseViewModel
{
    private readonly IStockTransferApiService _stockTransferApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<StockTransferSummaryDto> _transfers = new();

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedFromWarehouse;

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedToWarehouse;

    [ObservableProperty]
    private StockTransferStatus? _selectedStatus;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<StockTransferSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public StockTransfersViewModel(
        IStockTransferApiService stockTransferApiService,
        IWarehouseApiService warehouseApiService)
    {
        _stockTransferApiService = stockTransferApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadWarehousesAsync();
        await LoadTransfersAsync();
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
    public async Task LoadTransfersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _stockTransferApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                fromWarehouseId: SelectedFromWarehouse?.Id,
                toWarehouseId: SelectedToWarehouse?.Id,
                status: SelectedStatus,
                search: SearchQuery);

            if (res.Success && res.Data != null)
            {
                Transfers = new ObservableCollection<StockTransferSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل أوامر التحويل المخزني";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadTransfersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadTransfersAsync();
        }
    }

    [RelayCommand]
    private async Task CreateTransferAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadTransfersAsync();
        }
    }

    [RelayCommand]
    private async Task EditTransferAsync(object? parameter)
    {
        if (parameter is StockTransferSummaryDto transfer && OpenDialogHandler != null)
        {
            await OpenDialogHandler(transfer);
            await LoadTransfersAsync();
        }
    }

    [RelayCommand]
    private async Task UpdateStatusAsync(object? parameter)
    {
        if (parameter is not StockTransferSummaryDto transfer) return;

        StockTransferStatus nextStatus = transfer.Status switch
        {
            StockTransferStatus.Draft => StockTransferStatus.Confirmed,
            StockTransferStatus.Confirmed => StockTransferStatus.Completed,
            _ => StockTransferStatus.Confirmed
        };

        await ExecuteAsync(async () =>
        {
            var res = await _stockTransferApiService.UpdateStatusAsync(transfer.Id, nextStatus);
            if (res.Success) await LoadTransfersAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task DeleteTransferAsync(object? parameter)
    {
        if (parameter is not StockTransferSummaryDto transfer) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف التحويل", $"هل أنت متأكد من حذف أمر التحويل رقم '{transfer.TransferNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _stockTransferApiService.DeleteAsync(transfer.Id);
            if (res.Success) await LoadTransfersAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class StockTransferFormViewModel : BaseViewModel
{
    private readonly IStockTransferApiService _stockTransferApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductApiService _productApiService;

    [ObservableProperty]
    private Guid? _transferId;

    [ObservableProperty]
    private string _transferNumber = string.Empty;

    [ObservableProperty]
    private DateTime _transferDate = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedFromWarehouse;

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedToWarehouse;

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedProductToAdd;

    [ObservableProperty]
    private int _quantityToAdd = 1;

    [ObservableProperty]
    private ObservableCollection<CreateStockTransferItemRequest> _items = new();

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isViewOnly;

    public Action? CloseWindowHandler { get; set; }

    public StockTransferFormViewModel(
        IStockTransferApiService stockTransferApiService,
        IWarehouseApiService warehouseApiService,
        IProductApiService productApiService)
    {
        _stockTransferApiService = stockTransferApiService;
        _warehouseApiService = warehouseApiService;
        _productApiService = productApiService;
    }

    public async Task InitializeAsync(Guid? id)
    {
        await LoadLookupDataAsync();

        if (id.HasValue)
        {
            TransferId = id.Value;
            await LoadTransferDetailsAsync(id.Value);
        }
        else
        {
            IsViewOnly = false;
            TransferId = null;
            TransferNumber = $"TRF-{DateTime.Now:yyyyMMddHHmmss}";
            TransferDate = DateTime.Now;
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
            if (SelectedFromWarehouse == null && Warehouses.Count > 0) SelectedFromWarehouse = Warehouses[0];
            if (SelectedToWarehouse == null && Warehouses.Count > 1) SelectedToWarehouse = Warehouses[1];
        }

        var pRes = await _productApiService.GetAllAsync();
        if (pRes.Success && pRes.Data != null)
        {
            AvailableProducts = new ObservableCollection<ProductDto>(pRes.Data);
        }
    }

    private async Task LoadTransferDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _stockTransferApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                var trf = res.Data;
                TransferNumber = trf.TransferNumber;
                TransferDate = trf.TransferDate;
                Notes = trf.Notes;
                IsViewOnly = (trf.Status == StockTransferStatus.Completed);

                SelectedFromWarehouse = Warehouses.FirstOrDefault(w => w.Id == trf.FromWarehouseId);
                SelectedToWarehouse = Warehouses.FirstOrDefault(w => w.Id == trf.ToWarehouseId);

                Items = new ObservableCollection<CreateStockTransferItemRequest>(
                    trf.Items.Select(i => new CreateStockTransferItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductBarCodeId = i.ProductBarCodeId,
                        Quantity = i.Quantity,
                        Notes = i.Notes
                    }));
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل أمر التحويل";
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

        if (QuantityToAdd <= 0)
        {
            ErrorMessage = "الكمية يجب أن تكون أكبر من صفر";
            return;
        }

        var item = new CreateStockTransferItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            Quantity = QuantityToAdd
        };

        Items.Add(item);
        SelectedProductToAdd = null;
        QuantityToAdd = 1;
        ErrorMessage = null;
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is CreateStockTransferItemRequest item)
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedFromWarehouse == null)
        {
            ErrorMessage = "يجب اختيار المستودع المصدر";
            return;
        }

        if (SelectedToWarehouse == null)
        {
            ErrorMessage = "يجب اختيار المستودع الوجهة";
            return;
        }

        if (SelectedFromWarehouse.Id == SelectedToWarehouse.Id)
        {
            ErrorMessage = "لا يمكن التحويل لنفس المستودع أو الصالة";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل لأمر التحويل";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (TransferId.HasValue)
            {
                var req = new UpdateStockTransferRequest
                {
                    Notes = Notes
                };
                var res = await _stockTransferApiService.UpdateAsync(TransferId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل تعديل أمر التحويل";
            }
            else
            {
                var req = new CreateStockTransferRequest
                {
                    TransferNumber = TransferNumber,
                    FromWarehouseId = SelectedFromWarehouse.Id,
                    ToWarehouseId = SelectedToWarehouse.Id,
                    TransferDate = TransferDate,
                    Notes = Notes,
                    Items = Items.ToList()
                };
                var res = await _stockTransferApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل إنشاء أمر التحويل";
            }
        });
    }
}
