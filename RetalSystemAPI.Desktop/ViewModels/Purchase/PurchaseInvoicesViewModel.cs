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

public partial class PurchaseInvoicesViewModel : BaseViewModel
{
    private readonly IPurchaseInvoiceApiService _invoiceApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly ISupplierApiService _supplierApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty]
    private ObservableCollection<PurchaseInvoiceSummaryDto> _invoices = new();

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
    private InvoiceStatus? _selectedStatus;

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private PurchaseInvoiceSummaryDto? _selectedInvoice;

    public Func<PurchaseInvoiceDto?, Task<bool>>? OpenFormHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public PurchaseInvoicesViewModel(
        IPurchaseInvoiceApiService invoiceApiService,
        IBranchApiService branchApiService,
        ISupplierApiService supplierApiService,
        IWarehouseApiService warehouseApiService)
    {
        _invoiceApiService = invoiceApiService;
        _branchApiService = branchApiService;
        _supplierApiService = supplierApiService;
        _warehouseApiService = warehouseApiService;

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadLookupDataAsync();
        await LoadInvoicesAsync();
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
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
        }
    }

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _invoiceApiService.GetPagedAsync(
                pageNumber: CurrentPage,
                pageSize: PageSize,
                supplierId: SelectedSupplier?.Id,
                branchId: SelectedBranch?.Id,
                warehouseId: SelectedWarehouse?.Id,
                status: SelectedStatus,
                search: SearchTerm);

            if (res.Success && res.Data != null)
            {
                Invoices = new ObservableCollection<PurchaseInvoiceSummaryDto>(res.Data.Items);
                TotalCount = res.Data.TotalCount;
                TotalPages = Math.Max(1, res.Data.TotalPages);
                HasPreviousPage = res.Data.HasPreviousPage;
                HasNextPage = res.Data.HasNextPage;
            }
        });
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadInvoicesAsync();
    }

    [RelayCommand]
    private async Task ResetFilterAsync()
    {
        SelectedBranch = null;
        SelectedSupplier = null;
        SelectedWarehouse = null;
        SelectedStatus = null;
        SearchTerm = string.Empty;
        CurrentPage = 1;
        await LoadInvoicesAsync();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task CreateInvoiceAsync()
    {
        if (OpenFormHandler != null)
        {
            var success = await OpenFormHandler(null);
            if (success) await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task ViewDetailsAsync(PurchaseInvoiceSummaryDto? summary)
    {
        if (summary == null) return;
        await ExecuteAsync(async () =>
        {
            var res = await _invoiceApiService.GetByIdAsync(summary.Id);
            if (res.Success && res.Data != null && OpenFormHandler != null)
            {
                var success = await OpenFormHandler(res.Data);
                if (success) await LoadInvoicesAsync();
            }
        });
    }

    [RelayCommand]
    private async Task CancelInvoiceAsync(PurchaseInvoiceSummaryDto? summary)
    {
        if (summary == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد الإلغاء", $"هل أنت متأكد من إلغاء فاتورة المشتريات رقم '{summary.InvoiceNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _invoiceApiService.CancelAsync(summary.Id);
            if (res.Success)
            {
                await LoadInvoicesAsync();
            }
        });
    }

    [RelayCommand]
    private async Task DeleteInvoiceAsync(PurchaseInvoiceSummaryDto? summary)
    {
        if (summary == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد الحذف", $"هل أنت متأكد من حذف فاتورة المشتريات رقم '{summary.InvoiceNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _invoiceApiService.DeleteAsync(summary.Id);
            if (res.Success)
            {
                await LoadInvoicesAsync();
            }
        });
    }
}

public partial class PurchaseInvoiceFormViewModel : BaseViewModel
{
    private readonly IPurchaseInvoiceApiService _invoiceApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly ISupplierApiService _supplierApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IProductApiService _productApiService;

    public Guid? Id { get; private set; }
    public bool IsEditMode => Id.HasValue;

    [ObservableProperty]
    private string _invoiceNumber = $"PINV-{DateTime.Now:yyyyMMddHHmmss}";

    [ObservableProperty]
    private DateTime _invoiceDate = DateTime.Now;

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
    private InvoiceStatus _status = InvoiceStatus.Paid;

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

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
    private ObservableCollection<CreatePurchaseInvoiceItemRequest> _items = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> _availableProducts = new();

    [ObservableProperty]
    private ProductDto? _selectedProductToAdd;

    [ObservableProperty]
    private decimal _quantityToAdd = 1;

    [ObservableProperty]
    private decimal _unitPriceToAdd;

    [ObservableProperty]
    private decimal _itemDiscountToAdd;

    public Action? CloseAction { get; set; }
    public bool SaveSuccessful { get; private set; }

    public PurchaseInvoiceFormViewModel(
        IPurchaseInvoiceApiService invoiceApiService,
        IBranchApiService branchApiService,
        ISupplierApiService supplierApiService,
        IWarehouseApiService warehouseApiService,
        IProductApiService productApiService)
    {
        _invoiceApiService = invoiceApiService;
        _branchApiService = branchApiService;
        _supplierApiService = supplierApiService;
        _warehouseApiService = warehouseApiService;
        _productApiService = productApiService;
    }

    partial void OnDiscountAmountChanged(decimal value) => RecalculateTotals();
    partial void OnTaxAmountChanged(decimal value) => RecalculateTotals();
    partial void OnPaidAmountChanged(decimal value) => RecalculateTotals();

    partial void OnSelectedProductToAddChanged(ProductDto? value)
    {
        if (value != null)
        {
            UnitPriceToAdd = value.CostPrice > 0 ? value.CostPrice : value.SalePrice;
        }
    }

    public async Task InitializeAsync(PurchaseInvoiceDto? invoice = null)
    {
        await ExecuteAsync(async () =>
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
                Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
            }

            var prodRes = await _productApiService.GetAllAsync();
            if (prodRes.Success && prodRes.Data != null)
            {
                AvailableProducts = new ObservableCollection<ProductDto>(prodRes.Data);
            }

            if (invoice != null)
            {
                Id = invoice.Id;
                InvoiceNumber = invoice.InvoiceNumber;
                InvoiceDate = invoice.InvoiceDate;
                SelectedBranch = Branches.FirstOrDefault(b => b.Id == invoice.BranchId);
                SelectedSupplier = Suppliers.FirstOrDefault(s => s.Id == invoice.SupplierId);
                SelectedWarehouse = Warehouses.FirstOrDefault(w => w.Id == invoice.WarehouseId);
                Status = invoice.Status;
                PaymentMethod = invoice.PaymentMethod;
                SubTotal = invoice.SubTotal;
                DiscountAmount = invoice.DiscountAmount;
                TaxAmount = invoice.TaxAmount;
                TotalAmount = invoice.TotalAmount;
                PaidAmount = invoice.PaidAmount;
                RemainingAmount = invoice.RemainingAmount;
                Notes = invoice.Notes;

                Items = new ObservableCollection<CreatePurchaseInvoiceItemRequest>(
                    invoice.Items.Select(i => new CreatePurchaseInvoiceItemRequest
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        ProductBarCodeId = i.ProductBarCodeId,
                        BarCode = i.BarCode,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount
                    }));
            }
            else
            {
                SelectedBranch = Branches.FirstOrDefault();
                SelectedSupplier = Suppliers.FirstOrDefault();
                SelectedWarehouse = Warehouses.FirstOrDefault();
            }

            RecalculateTotals();
        });
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
            ErrorMessage = "الكمية يجب أن تكون أكبر من صفر";
            return;
        }

        var barCodeObj = SelectedProductToAdd.BarCodes?.FirstOrDefault();

        var item = new CreatePurchaseInvoiceItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            ProductBarCodeId = barCodeObj?.Id,
            BarCode = barCodeObj?.BarCode ?? SelectedProductToAdd.DefaultBarCode,
            Quantity = QuantityToAdd,
            UnitPrice = UnitPriceToAdd,
            DiscountAmount = ItemDiscountToAdd
        };

        Items.Add(item);

        SelectedProductToAdd = null;
        QuantityToAdd = 1;
        UnitPriceToAdd = 0;
        ItemDiscountToAdd = 0;
        ErrorMessage = null;

        RecalculateTotals();
    }

    [RelayCommand]
    private void RemoveItem(object? parameter)
    {
        if (parameter is CreatePurchaseInvoiceItemRequest item)
        {
            Items.Remove(item);
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = Items.Sum(i => i.LineTotal);
        TotalAmount = Math.Max(0, SubTotal - DiscountAmount + TaxAmount);

        if (Status == InvoiceStatus.Paid && PaidAmount < TotalAmount)
        {
            PaidAmount = TotalAmount;
        }

        RemainingAmount = Math.Max(0, TotalAmount - PaidAmount);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(InvoiceNumber))
        {
            ErrorMessage = "رقم الفاتورة مطلوب";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "يرجى اختيار الفرع المستلم";
            return;
        }

        if (SelectedSupplier == null)
        {
            ErrorMessage = "يرجى اختيار المورد";
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يرجى اختيار المخزن أو الصالة المستلمة للبضاعة";
            return;
        }

        if (!Items.Any())
        {
            ErrorMessage = "يجب إضافة صنف واحد على الأقل داخل الفاتورة";
            return;
        }

        RecalculateTotals();

        await ExecuteAsync(async () =>
        {
            if (IsEditMode)
            {
                var updateReq = new UpdatePurchaseInvoiceRequest
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = InvoiceDate,
                    BranchId = SelectedBranch.Id,
                    SupplierId = SelectedSupplier.Id,
                    WarehouseId = SelectedWarehouse.Id,
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

                var res = await _invoiceApiService.UpdateAsync(Id!.Value, updateReq);
                if (res.Success)
                {
                    SaveSuccessful = true;
                    CloseAction?.Invoke();
                }
                else
                {
                    ErrorMessage = res.Message;
                }
            }
            else
            {
                var createReq = new CreatePurchaseInvoiceRequest
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = InvoiceDate,
                    BranchId = SelectedBranch.Id,
                    SupplierId = SelectedSupplier.Id,
                    WarehouseId = SelectedWarehouse.Id,
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

                var res = await _invoiceApiService.CreateAsync(createReq);
                if (res.Success)
                {
                    SaveSuccessful = true;
                    CloseAction?.Invoke();
                }
                else
                {
                    ErrorMessage = res.Message;
                }
            }
        });
    }

    [RelayCommand]
    private void Cancel() => CloseAction?.Invoke();
}
