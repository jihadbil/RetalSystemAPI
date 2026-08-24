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

public partial class SalesInvoicesViewModel : BaseViewModel
{
    private readonly ISalesInvoiceApiService _salesInvoiceApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<SalesInvoiceSummaryDto> _invoices = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private InvoiceStatus? _selectedStatus;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<SalesInvoiceSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public SalesInvoicesViewModel(
        ISalesInvoiceApiService salesInvoiceApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService)
    {
        _salesInvoiceApiService = salesInvoiceApiService;
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAndWarehousesAsync();
        await LoadInvoicesAsync();
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
    public async Task LoadInvoicesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _salesInvoiceApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                branchId: SelectedBranch?.Id,
                warehouseId: SelectedWarehouse?.Id,
                status: SelectedStatus,
                search: SearchQuery);

            if (res.Success && res.Data != null)
            {
                Invoices = new ObservableCollection<SalesInvoiceSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل فواتير المبيعات";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task CreateInvoiceAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task EditInvoiceAsync(object? parameter)
    {
        if (parameter is SalesInvoiceSummaryDto invoice && OpenDialogHandler != null)
        {
            await OpenDialogHandler(invoice);
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task UpdateStatusAsync(object? parameter)
    {
        if (parameter is not SalesInvoiceSummaryDto invoice) return;

        InvoiceStatus nextStatus = invoice.Status switch
        {
            InvoiceStatus.Draft => InvoiceStatus.Issued,
            InvoiceStatus.Issued => InvoiceStatus.Paid,
            _ => InvoiceStatus.Issued
        };

        await ExecuteAsync(async () =>
        {
            var res = await _salesInvoiceApiService.UpdateStatusAsync(invoice.Id, nextStatus);
            if (res.Success) await LoadInvoicesAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task DeleteInvoiceAsync(object? parameter)
    {
        if (parameter is not SalesInvoiceSummaryDto invoice) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف الفاتورة", $"هل أنت متأكد من حذف الفاتورة رقم '{invoice.InvoiceNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _salesInvoiceApiService.DeleteAsync(invoice.Id);
            if (res.Success) await LoadInvoicesAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class SalesInvoiceFormViewModel : BaseViewModel
{
    private readonly ISalesInvoiceApiService _salesInvoiceApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly ICustomerApiService _customerApiService;
    private readonly IProductApiService _productApiService;

    [ObservableProperty]
    private Guid? _invoiceId;

    [ObservableProperty]
    private string _invoiceNumber = string.Empty;

    [ObservableProperty]
    private DateTime _invoiceDate = DateTime.Now;

    [ObservableProperty]
    private DateTime? _dueDate;

    [ObservableProperty]
    private InvoiceStatus _status = InvoiceStatus.Issued;

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

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
    private decimal _discountToAdd;

    [ObservableProperty]
    private ObservableCollection<CreateSalesInvoiceItemRequest> _items = new();

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _paidAmount;

    [ObservableProperty]
    private decimal _remainingAmount;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public SalesInvoiceFormViewModel(
        ISalesInvoiceApiService salesInvoiceApiService,
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        ICustomerApiService customerApiService,
        IProductApiService productApiService)
    {
        _salesInvoiceApiService = salesInvoiceApiService;
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

    partial void OnDiscountAmountChanged(decimal value) => RecalculateTotals();
    partial void OnTaxAmountChanged(decimal value) => RecalculateTotals();
    partial void OnPaidAmountChanged(decimal value) => RecalculateTotals();

    public async Task InitializeAsync(Guid? id)
    {
        await LoadLookupDataAsync();

        if (id.HasValue)
        {
            IsEditMode = true;
            InvoiceId = id.Value;
            await LoadInvoiceDetailsAsync(id.Value);
        }
        else
        {
            IsEditMode = false;
            InvoiceId = null;
            InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";
            InvoiceDate = DateTime.Now;
            Status = InvoiceStatus.Issued;
            PaymentMethod = PaymentMethod.Cash;
            Items.Clear();
            SubTotal = 0;
            DiscountAmount = 0;
            TaxAmount = 0;
            TotalAmount = 0;
            PaidAmount = 0;
            RemainingAmount = 0;
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

    private async Task LoadInvoiceDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _salesInvoiceApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                var inv = res.Data;
                InvoiceNumber = inv.InvoiceNumber;
                InvoiceDate = inv.InvoiceDate;
                DueDate = inv.DueDate;
                Status = inv.Status;
                PaymentMethod = inv.PaymentMethod;
                Notes = inv.Notes;
                PaidAmount = inv.PaidAmount;

                SelectedBranch = Branches.FirstOrDefault(b => b.Id == inv.BranchId);
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == inv.WarehouseId);
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == inv.CustomerId);

                Items = new ObservableCollection<CreateSalesInvoiceItemRequest>(
                    inv.Items.Select(i => new CreateSalesInvoiceItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductBarCodeId = i.ProductBarCodeId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount
                    }));

                SubTotal = inv.SubTotal;
                DiscountAmount = inv.DiscountAmount;
                TaxAmount = inv.TaxAmount;
                TotalAmount = inv.TotalAmount;
                RemainingAmount = inv.RemainingAmount;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل الفاتورة";
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

        var item = new CreateSalesInvoiceItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            Quantity = QuantityToAdd,
            UnitPrice = UnitPriceToAdd,
            DiscountAmount = DiscountToAdd
        };

        Items.Add(item);
        SelectedProductToAdd = null;
        QuantityToAdd = 1;
        UnitPriceToAdd = 0;
        DiscountToAdd = 0;
        ErrorMessage = null;

        RecalculateTotals();
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is CreateSalesInvoiceItemRequest item)
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.LineTotal);
        TotalAmount = SubTotal - DiscountAmount + TaxAmount;
        if (TotalAmount < 0) TotalAmount = 0;
        RemainingAmount = TotalAmount - PaidAmount;
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
            ErrorMessage = "يجب اختيار صالة العرض أو المخزن";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل للفاتورة";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && InvoiceId.HasValue)
            {
                var req = new UpdateSalesInvoiceRequest
                {
                    Status = Status,
                    PaymentMethod = PaymentMethod,
                    PaidAmount = PaidAmount,
                    Notes = Notes
                };
                var res = await _salesInvoiceApiService.UpdateAsync(InvoiceId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل تعديل الفاتورة";
            }
            else
            {
                var req = new CreateSalesInvoiceRequest
                {
                    InvoiceNumber = InvoiceNumber,
                    BranchId = SelectedBranch.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    CustomerId = SelectedCustomer?.Id,
                    InvoiceDate = InvoiceDate,
                    DueDate = DueDate,
                    Status = Status,
                    PaymentMethod = PaymentMethod,
                    SubTotal = SubTotal,
                    DiscountAmount = DiscountAmount,
                    TaxAmount = TaxAmount,
                    TotalAmount = TotalAmount,
                    PaidAmount = PaidAmount,
                    Notes = Notes,
                    Items = Items.ToList()
                };
                var res = await _salesInvoiceApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل إصدار الفاتورة";
            }
        });
    }
}
