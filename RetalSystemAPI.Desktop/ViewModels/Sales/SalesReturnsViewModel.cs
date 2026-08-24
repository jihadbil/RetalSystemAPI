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
        await ExecuteAsync(async () =>
        {
            var res = await _salesReturnApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                branchId: SelectedBranch?.Id,
                warehouseId: SelectedWarehouse?.Id,
                reason: SelectedReason,
                search: SearchQuery);

            if (res.Success && res.Data != null)
            {
                Returns = new ObservableCollection<SalesReturnSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل مرتجعات المبيعات";
            }
        });
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
}

public partial class SalesReturnFormViewModel : BaseViewModel
{
    private readonly ISalesReturnApiService _salesReturnApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly ICustomerApiService _customerApiService;
    private readonly IProductApiService _productApiService;

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

    public Action? CloseWindowHandler { get; set; }

    public SalesReturnFormViewModel(
        ISalesReturnApiService salesReturnApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        ICustomerApiService customerApiService,
        IProductApiService productApiService)
    {
        _salesReturnApiService = salesReturnApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _customerApiService = customerApiService;
        _productApiService = productApiService;
    }

    partial void OnSelectedProductToAddChanged(ProductDto? value)
    {
        if (value != null)
        {
            UnitPriceToAdd = value.SalePrice;
        }
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

        var pRes = await _productApiService.GetAllAsync();
        if (pRes.Success && pRes.Data != null)
        {
            AvailableProducts = new ObservableCollection<ProductDto>(pRes.Data);
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
