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
    private int _loadVersion;

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
        var version = ++_loadVersion;
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
            if (version != _loadVersion) return;

            if (res.Success && res.Data != null)
            {
                Invoices = new ObservableCollection<PurchaseInvoiceSummaryDto>(res.Data.Items);
                TotalCount = res.Data.TotalCount;
                TotalPages = Math.Max(1, res.Data.TotalPages);
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadInvoicesAsync();
                    return;
                }
                HasPreviousPage = res.Data.HasPreviousPage;
                HasNextPage = res.Data.HasNextPage;
            }
            else ErrorMessage = res.Message ?? "فشل تحميل فواتير المشتريات. أعد المحاولة.";
        }, isCurrent: () => version == _loadVersion);
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
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadInvoicesAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
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
        if (OpenFormHandler != null)
        {
            await OpenFormHandler(null);
            await LoadInvoicesAsync();
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
                await OpenFormHandler(res.Data);
                await LoadInvoicesAsync();
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
    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        CurrentPage = 1;
        _ = LoadInvoicesAsync();
    }
    partial void OnSelectedSupplierChanged(SupplierSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadInvoicesAsync();
    }
    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        CurrentPage = 1;
        _ = LoadInvoicesAsync();
    }
    partial void OnSelectedStatusChanged(InvoiceStatus? value)
    {
        CurrentPage = 1;
        _ = LoadInvoicesAsync();
    }
    partial void OnSearchTermChanged(string value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadInvoicesAsync);
    }
    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadInvoicesAsync();
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
    private InvoiceStatus _status = InvoiceStatus.Pending;

    public bool CanEdit => Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled && Status != InvoiceStatus.Voided;
    public bool IsClosed => !CanEdit;
    public string StatusDisplayName => Status switch
    {
        InvoiceStatus.Paid => "مغلقة ومرحلة 🔒",
        InvoiceStatus.Pending => "مفتوحة قيد الإدخال 🟢",
        InvoiceStatus.Draft => "مسودة مفتوحة 📝",
        InvoiceStatus.Cancelled => "ملغاة ❌",
        _ => Status.ToString()
    };

    partial void OnStatusChanged(InvoiceStatus value)
    {
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(IsClosed));
        OnPropertyChanged(nameof(StatusDisplayName));
    }

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

    // ── Barcode & Product Selection ──────────────────────────────
    [ObservableProperty]
    private string _searchBarcode = string.Empty;

    [ObservableProperty]
    private ProductDto? _selectedProductToAdd;

    // ── Reference Product Data ──────────────────────────────────
    [ObservableProperty]
    private decimal _previousCostPrice;

    [ObservableProperty]
    private decimal _productAverageCost;

    [ObservableProperty]
    private decimal _currentSalePrice;

    // ── Packaging & Unit Options ────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<PackagingUnitOption> _packagingOptions = new();

    [ObservableProperty]
    private PackagingUnitOption? _selectedPackagingOption;

    [ObservableProperty]
    private int _unitsPerPackage = 1;

    [ObservableProperty]
    private decimal _packageQuantity = 1;

    [ObservableProperty]
    private decimal _totalPieceQuantity = 1;

    // ── Invoice Price & Calculations ────────────────────────────
    [ObservableProperty]
    private decimal _invoiceEnteredPrice;

    [ObservableProperty]
    private decimal _calculatedPieceCost;

    [ObservableProperty]
    private decimal _itemDiscountToAdd;

    // ── Sale Price & Profit Analytics ───────────────────────────
    [ObservableProperty]
    private decimal _targetSalePrice;

    [ObservableProperty]
    private decimal _profitFromCurrentCost;

    [ObservableProperty]
    private decimal _profitMarginCurrentPercentage;

    [ObservableProperty]
    private decimal _profitFromAverageCost;

    [ObservableProperty]
    private decimal _profitMarginAveragePercentage;

    // ── Flavor Breakdown Distribution ───────────────────────────
    [ObservableProperty]
    private ObservableCollection<FlavorBreakdownItemViewModel> _currentFlavorBreakdowns = new();

    [ObservableProperty]
    private bool _hasMultipleFlavors;

    public Action? CloseAction { get; set; }
    public bool SaveSuccessful { get; private set; }

    public Func<string?, Task<ProductDto?>>? OpenProductDialogHandler { get; set; }
    public Func<Task<UnitDto?>>? OpenUnitDialogHandler { get; set; }
    public Func<Task<CategoryDto?>>? OpenCategoryDialogHandler { get; set; }

    /// <summary>فتح نموذج مرتجع مشتريات مربوط بهذه الفاتورة مع بند مبدئي — يوفره المحتوي (العرض) عند فتح النافذة</summary>
    public Func<Guid, CreatePurchaseInvoiceItemRequest, Task>? OpenReturnDialogHandler { get; set; }

    [RelayCommand]
    private async Task QuickCreateProductAsync()
    {
        if (OpenProductDialogHandler != null)
        {
            var created = await OpenProductDialogHandler(SearchBarcode);
            if (created != null)
            {
                SelectedProductToAdd = created;
                SearchBarcode = created.DefaultBarCode ?? created.Code ?? string.Empty;
                ErrorMessage = null;
            }
        }
    }

    [RelayCommand]
    private async Task QuickCreateUnitAsync()
    {
        if (OpenUnitDialogHandler != null)
        {
            var created = await OpenUnitDialogHandler();
            if (created != null)
            {
                var opt = new PackagingUnitOption { Name = created.Name, ConversionFactor = 1 };
                if (!PackagingOptions.Any(o => o.Name == created.Name))
                {
                    int insertIdx = Math.Max(0, PackagingOptions.Count - 1);
                    PackagingOptions.Insert(insertIdx, opt);
                }
                SelectedPackagingOption = opt;
                UnitsPerPackage = 1;
            }
        }
    }

    [RelayCommand]
    private async Task QuickCreateCategoryAsync()
    {
        if (OpenCategoryDialogHandler != null)
        {
            await OpenCategoryDialogHandler();
        }
    }

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
            PreviousCostPrice = value.CostPrice;
            ProductAverageCost = value.AveragePrice > 0 ? value.AveragePrice : value.CostPrice;
            CurrentSalePrice = value.SalePrice;
            TargetSalePrice = value.SalePrice > 0 ? value.SalePrice : (value.CostPrice * 1.25m);

            // تجهيز خيارات التعبئة والتنزيل من وحدات الصنف الفعلية فقط —
            // لا تُحقن خيارات افتراضية (قطعة/دستة/صندوق 24) لم تُعرّف للصنف
            var options = new ObservableCollection<PackagingUnitOption>();
            PackagingUnitOption? optionToSelect = null;

            if (value.Units != null && value.Units.Count > 0)
            {
                foreach (var unit in value.Units)
                {
                    var factor = unit.ConversionFactor > 0 ? unit.ConversionFactor : 1;
                    if (!options.Any(o => o.ConversionFactor == factor))
                    {
                        options.Add(new PackagingUnitOption { Name = unit.UnitName, ConversionFactor = factor });
                    }
                }

                // اختيار وحدة الصنف الافتراضية إن وجدت وإلا أول وحدة
                var defaultUnit = value.Units.FirstOrDefault(u => u.IsDefault);
                optionToSelect = defaultUnit != null
                    ? options.FirstOrDefault(o => o.ConversionFactor == (defaultUnit.ConversionFactor > 0 ? defaultUnit.ConversionFactor : 1))
                    : options.FirstOrDefault();
            }
            else
            {
                // لا وحدات معرّفة للصنف: القطعة (1) كأساس حسابي ثابت
                options.Add(new PackagingUnitOption { Name = "قطعة / حبة", ConversionFactor = 1 });
                optionToSelect = options.First();
            }

            options.Add(new PackagingUnitOption { Name = "مخصص (يدوي)", ConversionFactor = 1 });

            PackagingOptions = options;
            SelectedPackagingOption = optionToSelect;
            UnitsPerPackage = SelectedPackagingOption.ConversionFactor;
            PackageQuantity = 1;

            var baseUnitCost = value.CostPrice > 0 ? value.CostPrice : value.SalePrice;
            InvoiceEnteredPrice = baseUnitCost * UnitsPerPackage;

            // إعداد توزيع النكهات إذا كان للصنف عدة باركودات / نكهات
            CurrentFlavorBreakdowns.Clear();
            if (value.BarCodes != null && value.BarCodes.Count > 1)
            {
                HasMultipleFlavors = true;
                foreach (var bc in value.BarCodes)
                {
                    var isScanned = !string.IsNullOrWhiteSpace(SearchBarcode) && bc.BarCode.Equals(SearchBarcode.Trim(), StringComparison.OrdinalIgnoreCase);
                    var itemVm = new FlavorBreakdownItemViewModel
                    {
                        ProductBarCodeId = bc.Id,
                        BarCode = bc.BarCode,
                        Title = string.IsNullOrWhiteSpace(bc.Title) ? (bc.Description ?? bc.BarCode) : bc.Title,
                        UnitsPerPackage = UnitsPerPackage,
                        UnitPrice = CalculatedPieceCost,
                        PackageQuantity = isScanned ? 1 : 0,
                        OnQuantityChangedCallback = () =>
                        {
                            if (HasMultipleFlavors)
                            {
                                var sum = CurrentFlavorBreakdowns.Sum(b => b.PackageQuantity);
                                PackageQuantity = sum > 0 ? sum : 1;
                                RecalculateItemCalculations();
                            }
                        }
                    };
                    CurrentFlavorBreakdowns.Add(itemVm);
                }
                var scannedSum = CurrentFlavorBreakdowns.Sum(b => b.PackageQuantity);
                if (scannedSum > 0) PackageQuantity = scannedSum;
            }
            else
            {
                HasMultipleFlavors = false;
            }

            RecalculateItemCalculations();
        }
        else
        {
            PreviousCostPrice = 0;
            ProductAverageCost = 0;
            CurrentSalePrice = 0;
            TargetSalePrice = 0;
            InvoiceEnteredPrice = 0;
            CalculatedPieceCost = 0;
            ProfitFromCurrentCost = 0;
            ProfitMarginCurrentPercentage = 0;
            ProfitFromAverageCost = 0;
            ProfitMarginAveragePercentage = 0;
            PackagingOptions.Clear();
            SelectedPackagingOption = null;
            HasMultipleFlavors = false;
            CurrentFlavorBreakdowns.Clear();
        }
    }

    partial void OnSelectedPackagingOptionChanged(PackagingUnitOption? value)
    {
        if (value != null && value.Name != "مخصص (يدوي)")
        {
            UnitsPerPackage = value.ConversionFactor;
            if (PreviousCostPrice > 0)
            {
                InvoiceEnteredPrice = PreviousCostPrice * UnitsPerPackage;
            }
        }
        RecalculateItemCalculations();
    }

    partial void OnUnitsPerPackageChanged(int value) => RecalculateItemCalculations();
    partial void OnPackageQuantityChanged(decimal value) => RecalculateItemCalculations();
    partial void OnInvoiceEnteredPriceChanged(decimal value) => RecalculateItemCalculations();
    partial void OnTargetSalePriceChanged(decimal value) => RecalculateItemCalculations();

    private void RecalculateItemCalculations()
    {
        var factor = UnitsPerPackage > 0 ? UnitsPerPackage : 1;
        TotalPieceQuantity = Math.Max(1, PackageQuantity * factor);
        CalculatedPieceCost = factor > 0 ? Math.Round(InvoiceEnteredPrice / factor, 4) : InvoiceEnteredPrice;

        foreach (var fb in CurrentFlavorBreakdowns)
        {
            fb.UnitsPerPackage = factor;
            fb.UnitPrice = CalculatedPieceCost;
        }

        // حساب الأرباح ونسبتها
        ProfitFromCurrentCost = TargetSalePrice - CalculatedPieceCost;
        ProfitMarginCurrentPercentage = CalculatedPieceCost > 0
            ? Math.Round((ProfitFromCurrentCost / CalculatedPieceCost) * 100, 2)
            : 0;

        var effectiveAvg = ProductAverageCost > 0 ? ProductAverageCost : CalculatedPieceCost;
        ProfitFromAverageCost = TargetSalePrice - effectiveAvg;
        ProfitMarginAveragePercentage = effectiveAvg > 0
            ? Math.Round((ProfitFromAverageCost / effectiveAvg) * 100, 2)
            : 0;
    }

    [RelayCommand]
    private async Task LookupBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchBarcode)) return;

        var barcodeTrimmed = SearchBarcode.Trim();

        // الاستعلام الفوري من السيرفر بمطابقة تامة للباركود فقط
        await ExecuteAsync(async () =>
        {
            var res = await _productApiService.GetByBarCodeAsync(barcodeTrimmed);
            if (res.Success && res.Data != null)
            {
                SelectedProductToAdd = res.Data;
                ErrorMessage = null;
            }
            else
            {
                SelectedProductToAdd = null;
                ErrorMessage = $"لم يتم العثور على صنف بالباركود: {barcodeTrimmed}";
            }
        });
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
                Status = InvoiceStatus.Pending;
            }

            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(IsClosed));
            OnPropertyChanged(nameof(StatusDisplayName));
            RecalculateTotals();
        });
    }

    [RelayCommand]
    private async Task AddItem()
    {
        if (!CanEdit)
        {
            ErrorMessage = "لا يمكن إضافة بنود لفاتورة مشتريات مغلقة أو ملغاة";
            return;
        }

        if (SelectedProductToAdd == null)
        {
            ErrorMessage = "يرجى مسح الباركود أو اختيار الصنف أولاً";
            return;
        }

        if (TotalPieceQuantity <= 0)
        {
            ErrorMessage = "الكمية الإجمالية يجب أن تكون أكبر من صفر";
            return;
        }

        if (CalculatedPieceCost < 0)
        {
            ErrorMessage = "سعر الشراء لا يمكن أن يكون سالباً";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "يرجى اختيار الفرع المستلم قبل إضافة البند لحفظ الفاتورة";
            return;
        }

        if (SelectedSupplier == null)
        {
            ErrorMessage = "يرجى اختيار المورد قبل إضافة البند لحفظ الفاتورة";
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يرجى اختيار المخزن أو الصالة المستلمة قبل إضافة البند لحفظ الفاتورة";
            return;
        }

        List<PurchaseInvoiceItemBreakdownRequest> breakdowns = new();
        if (HasMultipleFlavors && CurrentFlavorBreakdowns.Any(b => b.PackageQuantity > 0))
        {
            breakdowns = CurrentFlavorBreakdowns
                .Where(b => b.PackageQuantity > 0)
                .Select(b => new PurchaseInvoiceItemBreakdownRequest
                {
                    ProductBarCodeId = b.ProductBarCodeId,
                    BarCode = b.BarCode,
                    Title = b.Title,
                    PackageQuantity = b.PackageQuantity,
                    UnitsPerPackage = UnitsPerPackage,
                    UnitPrice = CalculatedPieceCost
                }).ToList();
        }
        else
        {
            var barCodeObj = SelectedProductToAdd.BarCodes?.FirstOrDefault(b => b.BarCode.Equals(SearchBarcode.Trim(), StringComparison.OrdinalIgnoreCase))
                             ?? SelectedProductToAdd.BarCodes?.FirstOrDefault();
            if (barCodeObj != null)
            {
                breakdowns.Add(new PurchaseInvoiceItemBreakdownRequest
                {
                    ProductBarCodeId = barCodeObj.Id,
                    BarCode = barCodeObj.BarCode,
                    Title = string.IsNullOrWhiteSpace(barCodeObj.Title) ? SelectedProductToAdd.Name : barCodeObj.Title,
                    PackageQuantity = PackageQuantity,
                    UnitsPerPackage = UnitsPerPackage,
                    UnitPrice = CalculatedPieceCost
                });
            }
        }

        var defaultBc = breakdowns.FirstOrDefault();
        var item = new CreatePurchaseInvoiceItemRequest
        {
            ProductId = SelectedProductToAdd.Id,
            ProductName = SelectedProductToAdd.Name,
            ProductBarCodeId = defaultBc?.ProductBarCodeId ?? SelectedProductToAdd.BarCodes?.FirstOrDefault()?.Id,
            BarCode = defaultBc?.BarCode ?? SelectedProductToAdd.DefaultBarCode ?? SearchBarcode,
            PackageUnitName = SelectedPackagingOption?.Name ?? "قطعة",
            PackageQuantity = PackageQuantity,
            UnitsPerPackage = UnitsPerPackage,
            InvoicePackagePrice = InvoiceEnteredPrice,
            Quantity = TotalPieceQuantity,
            UnitPrice = CalculatedPieceCost,
            SalePrice = TargetSalePrice,
            DiscountAmount = ItemDiscountToAdd,
            Breakdowns = breakdowns
        };

        Items.Add(item);

        // تنظيف الحقول لعملية الإدخال التالية
        SelectedProductToAdd = null;
        SearchBarcode = string.Empty;
        PackageQuantity = 1;
        UnitsPerPackage = 1;
        InvoiceEnteredPrice = 0;
        CalculatedPieceCost = 0;
        TargetSalePrice = 0;
        ItemDiscountToAdd = 0;
        HasMultipleFlavors = false;
        CurrentFlavorBreakdowns.Clear();
        ErrorMessage = null;

        RecalculateTotals();

        // ── الفتح والحفظ الفوري بالسيرفر مع كل بند ──
        await ExecuteAsync(async () =>
        {
            if (!Id.HasValue)
            {
                var createReq = new CreatePurchaseInvoiceRequest
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = InvoiceDate,
                    BranchId = SelectedBranch.Id,
                    SupplierId = SelectedSupplier.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    Status = InvoiceStatus.Pending,
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
                if (res.Success && res.Data != null)
                {
                    Id = res.Data.Id;
                    Status = res.Data.Status;
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(IsClosed));
                    OnPropertyChanged(nameof(StatusDisplayName));
                }
                else
                {
                    ErrorMessage = res.Message ?? "فشل فتح الفاتورة وحفظ البند الأول بالسيرفر";
                }
            }
            else
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

                var res = await _invoiceApiService.UpdateAsync(Id.Value, updateReq);
                if (!res.Success)
                {
                    ErrorMessage = res.Message ?? "فشل مزامنة بيانات الفاتورة المفتوحة بالسيرفر";
                }
            }
        });
    }

    [RelayCommand]
    private async Task RemoveItem(object? parameter)
    {
        if (!CanEdit)
        {
            ErrorMessage = "لا يمكن حذف بنود من فاتورة مغلقة";
            return;
        }

        if (parameter is CreatePurchaseInvoiceItemRequest item)
        {
            Items.Remove(item);
            RecalculateTotals();

            if (Id.HasValue && SelectedBranch != null && SelectedSupplier != null && SelectedWarehouse != null)
            {
                await ExecuteAsync(async () =>
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

                    var res = await _invoiceApiService.UpdateAsync(Id.Value, updateReq);
                    if (!res.Success)
                    {
                        ErrorMessage = res.Message;
                    }
                });
            }
        }
    }

    /// <summary>إنشاء مرتجع مشتريات مربوط بهذه الفاتورة بدءاً من البند المحدد — البديل الوحيد لحذف البند بعد الإغلاق النهائي</summary>
    [RelayCommand]
    private async Task CreateReturnFromItemAsync(object? parameter)
    {
        if (parameter is not CreatePurchaseInvoiceItemRequest item || !Id.HasValue) return;

        if (!IsClosed)
        {
            ErrorMessage = "المرتجع متاح فقط بعد ترحيل وإغلاق الفاتورة نهائياً — قبل الإغلاق استخدم حذف البند";
            return;
        }

        if (OpenReturnDialogHandler != null)
        {
            await OpenReturnDialogHandler(Id.Value, item);
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
        if (!CanEdit)
        {
            ErrorMessage = "الفاتورة مغلقة ومرحلة بالفعل ولا يمكن إعادة حفظها أو تعديلها";
            return;
        }

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

        // ── ترحيل وإغلاق الفاتورة نهائياً ──
        await ExecuteAsync(async () =>
        {
            Status = InvoiceStatus.Paid;

            if (IsEditMode && Id.HasValue)
            {
                var updateReq = new UpdatePurchaseInvoiceRequest
                {
                    InvoiceNumber = InvoiceNumber,
                    InvoiceDate = InvoiceDate,
                    BranchId = SelectedBranch.Id,
                    SupplierId = SelectedSupplier.Id,
                    WarehouseId = SelectedWarehouse.Id,
                    Status = InvoiceStatus.Paid,
                    PaymentMethod = PaymentMethod,
                    SubTotal = SubTotal,
                    DiscountAmount = DiscountAmount,
                    TaxAmount = TaxAmount,
                    TotalAmount = TotalAmount,
                    PaidAmount = TotalAmount,
                    Notes = Notes,
                    Items = Items.ToList()
                };

                var res = await _invoiceApiService.UpdateAsync(Id.Value, updateReq);
                if (res.Success)
                {
                    SaveSuccessful = true;
                    CloseAction?.Invoke();
                }
                else
                {
                    Status = InvoiceStatus.Pending;
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
                    Status = InvoiceStatus.Paid,
                    PaymentMethod = PaymentMethod,
                    SubTotal = SubTotal,
                    DiscountAmount = DiscountAmount,
                    TaxAmount = TaxAmount,
                    TotalAmount = TotalAmount,
                    PaidAmount = TotalAmount,
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
                    Status = InvoiceStatus.Pending;
                    ErrorMessage = res.Message;
                }
            }
        });
    }

    [RelayCommand]
    private void Cancel() => CloseAction?.Invoke();
}

public partial class FlavorBreakdownItemViewModel : ObservableObject
{
    public Guid ProductBarCodeId { get; set; }
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    [ObservableProperty]
    private decimal _packageQuantity = 0;

    [ObservableProperty]
    private int _unitsPerPackage = 1;

    [ObservableProperty]
    private decimal _unitPrice = 0;

    public decimal TotalPieces => PackageQuantity * (UnitsPerPackage > 0 ? UnitsPerPackage : 1);
    public decimal TotalCost => TotalPieces * UnitPrice;

    public Action? OnQuantityChangedCallback { get; set; }

    partial void OnPackageQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalPieces));
        OnPropertyChanged(nameof(TotalCost));
        OnQuantityChangedCallback?.Invoke();
    }

    partial void OnUnitsPerPackageChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPieces));
        OnPropertyChanged(nameof(TotalCost));
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalCost));
    }
}
