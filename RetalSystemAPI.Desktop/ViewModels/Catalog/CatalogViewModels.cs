using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Catalog;

public partial class CategoriesViewModel : BaseViewModel
{
    private readonly ICategoryApiService _categoryApiService;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> _categories = new();

    public Func<CategoryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public CategoriesViewModel(ICategoryApiService categoryApiService)
    {
        _categoryApiService = categoryApiService;
        _ = LoadCategoriesAsync();
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _categoryApiService.GetAllAsync();
            if (res.Success && res.Data != null)
                Categories = new ObservableCollection<CategoryDto>(res.Data);
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task CreateCategoryAsync()
    {
        if (OpenDialogHandler != null) { await OpenDialogHandler(null); await LoadCategoriesAsync(); }
    }

    [RelayCommand]
    private async Task EditCategoryAsync(CategoryDto? cat)
    {
        if (cat != null && OpenDialogHandler != null) { await OpenDialogHandler(cat); await LoadCategoriesAsync(); }
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryDto? cat)
    {
        if (cat == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف التصنيف", $"هل أنت تأكد من حذف التصنيف '{cat.Name}'؟ لا يمكن التراجع بعد الحذف.");
            if (!confirmed) return;
        }
        await ExecuteAsync(async () =>
        {
            var res = await _categoryApiService.DeleteAsync(cat.Id);
            if (res.Success) await LoadCategoriesAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class CategoryFormViewModel : BaseViewModel
{
    private readonly ICategoryApiService _categoryApiService;

    [ObservableProperty] private Guid? _categoryId;
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    private string _name = string.Empty;
    [ObservableProperty] private Guid? _parentCategoryId;
    [ObservableProperty] private ObservableCollection<CategoryDto> _availableParentCategories = new();
    [ObservableProperty] private bool _isEditMode;
    public CategoryDto? CreatedCategory { get; private set; }
    public Action? CloseWindowHandler { get; set; }

    public CategoryFormViewModel(ICategoryApiService categoryApiService)
    {
        _categoryApiService = categoryApiService;
    }

    public void Initialize(CategoryDto? cat)
    {
        CreatedCategory = null;
        if (cat != null) 
        { 
            IsEditMode = true; 
            CategoryId = cat.Id; 
            Name = cat.Name; 
            ParentCategoryId = cat.ParentCategoryId ?? Guid.Empty; 
        }
        else 
        { 
            IsEditMode = false; 
            CategoryId = null; 
            Name = string.Empty; 
            ParentCategoryId = Guid.Empty; 
        }

        _ = LoadParentCategoriesAsync(cat?.Id);
    }

    private async Task LoadParentCategoriesAsync(Guid? currentCatId)
    {
        var res = await _categoryApiService.GetAllAsync();
        var list = new ObservableCollection<CategoryDto>
        {
            new CategoryDto
            {
                Id = Guid.Empty,
                Name = "-- تصنيف رئيسي (بدون تصنيف أب) --"
            }
        };

        if (res.Success && res.Data != null)
        {
            foreach (var c in res.Data)
            {
                if (currentCatId.HasValue && c.Id == currentCatId.Value) continue;
                list.Add(c);
            }
        }

        AvailableParentCategories = list;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;
        if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "اسم التصنيف مطلوب"; return; }

        Guid? parentIdToSave = (ParentCategoryId.HasValue && ParentCategoryId.Value != Guid.Empty)
            ? ParentCategoryId.Value
            : null;

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && CategoryId.HasValue)
            {
                var res = await _categoryApiService.UpdateAsync(CategoryId.Value, new UpdateCategoryRequest { Name = Name, ParentCategoryId = parentIdToSave });
                if (res.Success)
                {
                    CreatedCategory = res.Data ?? new CategoryDto { Id = CategoryId.Value, Name = Name, ParentCategoryId = parentIdToSave };
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var res = await _categoryApiService.CreateAsync(new CreateCategoryRequest { Name = Name, ParentCategoryId = parentIdToSave });
                if (res.Success)
                {
                    CreatedCategory = res.Data ?? new CategoryDto { Id = Guid.NewGuid(), Name = Name, ParentCategoryId = parentIdToSave };
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
        });
    }
}

public partial class UnitsViewModel : BaseViewModel
{
    private readonly IUnitApiService _unitApiService;
    [ObservableProperty] private ObservableCollection<UnitDto> _units = new();
    public Func<UnitDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public UnitsViewModel(IUnitApiService unitApiService)
    {
        _unitApiService = unitApiService;
        _ = LoadUnitsAsync();
    }

    [RelayCommand]
    public async Task LoadUnitsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _unitApiService.GetAllAsync();
            if (res.Success && res.Data != null) Units = new ObservableCollection<UnitDto>(res.Data);
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task CreateUnitAsync()
    {
        if (OpenDialogHandler != null) { await OpenDialogHandler(null); await LoadUnitsAsync(); }
    }

    [RelayCommand]
    private async Task EditUnitAsync(UnitDto? unit)
    {
        if (unit != null && OpenDialogHandler != null) { await OpenDialogHandler(unit); await LoadUnitsAsync(); }
    }

    [RelayCommand]
    private async Task DeleteUnitAsync(UnitDto? unit)
    {
        if (unit == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف وحدة القياس", $"هل أنت تأكد من حذف وحدة القياس '{unit.Name}'؟ لا يمكن التراجع بعد الحذف.");
            if (!confirmed) return;
        }
        await ExecuteAsync(async () =>
        {
            var res = await _unitApiService.DeleteAsync(unit.Id);
            if (res.Success) await LoadUnitsAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class UnitFormViewModel : BaseViewModel
{
    private readonly IUnitApiService _unitApiService;
    [ObservableProperty] private Guid? _unitId;
    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "اسم الوحدة مطلوب")]
    private string _name = string.Empty;
    [ObservableProperty] private string? _symbol;
    [ObservableProperty] private bool _isEditMode;
    public UnitDto? CreatedUnit { get; private set; }
    public Action? CloseWindowHandler { get; set; }

    public UnitFormViewModel(IUnitApiService unitApiService)
    {
        _unitApiService = unitApiService;
    }

    public void Initialize(UnitDto? unit)
    {
        CreatedUnit = null;
        if (unit != null) { IsEditMode = true; UnitId = unit.Id; Name = unit.Name; Symbol = unit.Symbol; }
        else { IsEditMode = false; UnitId = null; Name = string.Empty; Symbol = string.Empty; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;
        if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "اسم الوحدة مطلوب"; return; }
        await ExecuteAsync(async () =>
        {
            if (IsEditMode && UnitId.HasValue)
            {
                var res = await _unitApiService.UpdateAsync(UnitId.Value, new UpdateUnitRequest { Name = Name, Symbol = Symbol });
                if (res.Success)
                {
                    CreatedUnit = res.Data ?? new UnitDto { Id = UnitId.Value, Name = Name, Symbol = Symbol };
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var res = await _unitApiService.CreateAsync(new CreateUnitRequest { Name = Name, Symbol = Symbol });
                if (res.Success)
                {
                    CreatedUnit = res.Data ?? new UnitDto { Id = Guid.NewGuid(), Name = Name, Symbol = Symbol };
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
        });
    }
}

public partial class ProductsViewModel : BaseViewModel
{
    private readonly IProductApiService _productApiService;
    [ObservableProperty] private ObservableCollection<ProductDto> _products = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private int _pageSize = 10;
    [ObservableProperty] private string _searchQuery = string.Empty;

    /// <summary>مطابقة الباركود تماماً — يقتصر البحث على التطابق التام مع الباركود بدلاً من الاحتواء</summary>
    [ObservableProperty] private bool _exactBarcodeMatch = false;

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadProductsAsync();
    }

    public Func<ProductDto?, Task>? OpenDialogHandler { get; set; }
    public Action<ProductDto>? OpenDetailHandler { get; set; }

    public ProductsViewModel(IProductApiService productApiService)
    {
        _productApiService = productApiService;
        _ = LoadProductsAsync();
    }

    [ObservableProperty] private int _totalCount;
    private int _loadVersion;
    private string _loadedQuery = string.Empty;
    public string EmptyTitle => string.IsNullOrWhiteSpace(_loadedQuery) ? "أضف أول منتج" : "لا توجد نتائج مطابقة";
    public string EmptyDescription => string.IsNullOrWhiteSpace(_loadedQuery) ? "ابدأ بإضافة منتج أو استيراد ملف Excel." : "جرّب اسمًا أو كودًا آخر، أو امسح البحث لعرض جميع المنتجات.";

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchQuery = string.Empty;
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        var version = ++_loadVersion;
        var query = SearchQuery.Trim();
        if (query != _loadedQuery) CurrentPage = 1;
        var page = CurrentPage;
        var pageSize = PageSize;
        _loadedQuery = query;
        IsLoading = true;
        ErrorMessage = null;
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyDescription));
        try
        {
            if (query.Length > 0)
            {
                var result = await _productApiService.SearchAsync(query);
                if (version != _loadVersion) return;
                if (!result.Success || result.Data == null) throw new InvalidOperationException("تعذر البحث عن المنتجات. أعد المحاولة.");

                // في وضع مطابقة الباركود التامة: يبقى فقط من يحمل باركوداً مطابقاً حرفياً
                if (ExactBarcodeMatch)
                {
                    result.Data = result.Data
                        .Where(p => p.BarCodes != null && p.BarCodes.Any(b => string.Equals(b.BarCode, query, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                TotalCount = result.Data.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / pageSize));
                CurrentPage = Math.Clamp(page, 1, TotalPages);
                Products = new ObservableCollection<ProductDto>(result.Data.Skip((CurrentPage - 1) * pageSize).Take(pageSize));
            }
            else
            {
                var result = await _productApiService.GetPagedAsync(page, pageSize);
                if (version != _loadVersion) return;
                if (!result.Success || result.Data == null) throw new InvalidOperationException("تعذر تحميل المنتجات. تحقق من الاتصال وأعد المحاولة.");
                TotalCount = result.Data.TotalCount;
                TotalPages = Math.Max(1, result.Data.TotalPages);
                if (page > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadProductsAsync();
                    return;
                }
                Products = new ObservableCollection<ProductDto>(result.Data.Items);
            }
        }
        catch (Exception)
        {
            if (version == _loadVersion) ErrorMessage = "تعذر تحميل النتائج. تحقق من الاتصال ثم اضغط إعادة المحاولة.";
        }
        finally { if (version == _loadVersion) IsLoading = false; }
    }
    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages) { CurrentPage++; await LoadProductsAsync(); }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1) { CurrentPage--; await LoadProductsAsync(); }
    }

    [RelayCommand]
    private async Task CreateProductAsync()
    {
        if (OpenDialogHandler != null) { await OpenDialogHandler(null); await LoadProductsAsync(); }
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductDto? p)
    {
        if (p != null && OpenDialogHandler != null)
        {
            var fullRes = await _productApiService.GetByIdAsync(p.Id);
            var productToEdit = (fullRes.Success && fullRes.Data != null) ? fullRes.Data : p;
            await OpenDialogHandler(productToEdit);
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    private void ViewDetail(ProductDto? p)
    {
        if (p != null) OpenDetailHandler?.Invoke(p);
    }

    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductDto? p)
    {
        if (p == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المنتج", $"هل أنت تأكد من حذف المنتج '{p.Name}'؟ لا يمكن التراجع بعد الحذف.");
            if (!confirmed) return;
        }
        await ExecuteAsync(async () =>
        {
            var res = await _productApiService.DeleteAsync(p.Id);
            if (res.Success) await LoadProductsAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "ملفات Excel (*.xlsx)|*.xlsx",
            FileName = $"الأصناف_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            Title = "تصدير الأصناف والباركودات والتصنيفات"
        };

        if (sfd.ShowDialog() == true)
        {
            await ExecuteAsync(async () =>
            {
                var bytes = await _productApiService.ExportExcelAsync();
                if (bytes != null && bytes.Length > 0)
                {
                    await System.IO.File.WriteAllBytesAsync(sfd.FileName, bytes);
                    System.Windows.MessageBox.Show("تم تصدير الأصناف والباركودات والتصنيفات بنجاح إلى ملف Excel!", "تم التصدير بنجاح", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "فشل في تصدير البيانات من النظام";
                }
            });
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        var sfd = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "ملفات Excel (*.xlsx)|*.xlsx",
            FileName = "قالب_استيراد_الأصناف.xlsx",
            Title = "تنزيل قالب استيراد الأصناف"
        };

        if (sfd.ShowDialog() == true)
        {
            await ExecuteAsync(async () =>
            {
                var bytes = await _productApiService.DownloadTemplateAsync();
                if (bytes != null && bytes.Length > 0)
                {
                    await System.IO.File.WriteAllBytesAsync(sfd.FileName, bytes);
                    System.Windows.MessageBox.Show("تم تنزيل قالب استيراد الأصناف بنجاح!", "تم تنزيل القالب", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "فشل في تنزيل القالب";
                }
            });
        }
    }

    public Func<Task>? OpenImportWizardHandler { get; set; }

    [RelayCommand]
    private async Task ImportExcelAsync()
    {
        if (OpenImportWizardHandler != null)
        {
            await OpenImportWizardHandler();
            await LoadProductsAsync();
        }
        else
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ملفات Excel (*.xlsx)|*.xlsx",
                Title = "اختر ملف Excel لاستيراد الأصناف"
            };

            if (ofd.ShowDialog() == true)
            {
                await ExecuteAsync(async () =>
                {
                    var res = await _productApiService.ImportExcelAsync(ofd.FileName);
                    if (res.Success && res.Data != null)
                    {
                        var data = res.Data;
                        string msg = $"تمت عملية الاستيراد بنجاح:\n" +
                                     $"• الأصناف المضافة/المحدثة: {data.ProductsImported}\n" +
                                     $"• الباركودات المضافة/المحدثة: {data.BarcodesImported}\n" +
                                     $"• التصنيفات المضافة/المحدثة: {data.CategoriesImported}";

                        if (data.Warnings.Count > 0)
                        {
                            msg += $"\n\nالتنبيهات ({data.Warnings.Count}):\n" + string.Join("\n", data.Warnings.Take(5));
                        }

                        System.Windows.MessageBox.Show(msg, "نتيجة الاستيراد", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        await LoadProductsAsync();
                    }
                    else
                    {
                        string err = res.Message ?? "حدث خطأ غير معروف أثناء عملية الاستيراد";
                        ErrorMessage = err;
                        System.Windows.MessageBox.Show($"فشلت عملية الاستيراد:\n{err}", "خطأ في الاستيراد", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                });
            }
        }
    }
}

public partial class ProductFormViewModel : BaseViewModel
{
    private readonly IProductApiService _productApiService;
    private readonly ICategoryApiService _categoryApiService;
    private readonly IUnitApiService _unitApiService;
    private readonly IProductImageApiService _productImageApiService;
    private readonly IWarehouseApiService _warehouseApiService;

    [ObservableProperty] private Guid? _productId;
    [ObservableProperty, NotifyDataErrorInfo, System.ComponentModel.DataAnnotations.Required(ErrorMessage = "اسم المنتج مطلوب")] private string _name = string.Empty;
    [ObservableProperty, NotifyDataErrorInfo, System.ComponentModel.DataAnnotations.Required(ErrorMessage = "كود المنتج مطلوب")] private string _code = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty, NotifyDataErrorInfo, System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "التكلفة يجب أن تكون صفرًا أو أكثر")] private decimal _costPrice;
    [ObservableProperty, NotifyDataErrorInfo, System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "سعر البيع يجب أن يكون صفرًا أو أكثر")] private decimal _salePrice;
    [ObservableProperty] private Guid _categoryId;

    [ObservableProperty] private ObservableCollection<CategoryDto> _categories = new();
    [ObservableProperty] private ObservableCollection<UnitDto> _units = new();
    [ObservableProperty] private ObservableCollection<WarehouseSummaryDto> _showrooms = new();
    [ObservableProperty] private WarehouseSummaryDto? _selectedShowroom;
    [ObservableProperty] private ObservableCollection<WarehouseSummaryDto> _storageWarehouses = new();
    [ObservableProperty] private WarehouseSummaryDto? _selectedStorageWarehouse;

    [ObservableProperty] private ObservableCollection<CreateShowroomStockQuantityRequest> _showroomInitialQuantities = new();
    [ObservableProperty] private ObservableCollection<CreateStorageStockQuantityRequest> _storageInitialQuantities = new();

    // Multi-Unit support
    [ObservableProperty] private ObservableCollection<CreateProductUnitRequest> _productUnits = new();
    [ObservableProperty] private Guid? _selectedAddUnitId;
    [ObservableProperty] private int _selectedAddConversionFactor = 1;
    [ObservableProperty] private bool _selectedAddIsDefault;

    [ObservableProperty] private int _initialShowroomQuantity;

    // Multi-Barcode support
    [ObservableProperty] private ObservableCollection<CreateProductBarCodeRequest> _productBarCodes = new();
    [ObservableProperty] private string _selectedAddBarCode = string.Empty;
    [ObservableProperty] private string _selectedAddBarCodeTitle = string.Empty;
    [ObservableProperty] private string? _selectedAddBarCodeDescription;
    [ObservableProperty] private int _selectedAddBarCodeInitialQuantity;

    // Product Images support
    [ObservableProperty] private ObservableCollection<ProductImageDto> _productImages = new();
    [ObservableProperty] private ObservableCollection<PendingProductImage> _pendingProductImages = new();
    [ObservableProperty] private ObservableCollection<ProductBarCodeDto> _existingBarCodesForImages = new();
    [ObservableProperty] private string _selectedUploadFilePath = string.Empty;
    [ObservableProperty] private bool _selectedUploadIsDefault;
    [ObservableProperty] private Guid? _selectedUploadBarcodeId;

    // حالة رفع الصور (شريط التقدم)
    [ObservableProperty] private bool _isUploadingImage;
    [ObservableProperty] private int _uploadProgressPercent;
    [ObservableProperty] private string _uploadStatusText = string.Empty;

    [ObservableProperty] private bool _isEditMode;
    public ProductDto? CreatedOrUpdatedProduct { get; private set; }
    public Func<Task<CategoryDto?>>? OpenCategoryDialogHandler { get; set; }
    public Func<Task<UnitDto?>>? OpenUnitDialogHandler { get; set; }
    public Action? CloseWindowHandler { get; set; }

    [RelayCommand]
    private async Task QuickCreateCategoryAsync()
    {
        if (OpenCategoryDialogHandler != null)
        {
            var cat = await OpenCategoryDialogHandler();
            if (cat != null)
            {
                var catRes = await _categoryApiService.GetAllAsync();
                if (catRes.Success && catRes.Data != null)
                {
                    Categories = new ObservableCollection<CategoryDto>(catRes.Data);
                }
                else if (!Categories.Any(c => c.Id == cat.Id))
                {
                    Categories.Add(cat);
                }
                CategoryId = cat.Id;
            }
        }
    }

    [RelayCommand]
    private async Task QuickCreateUnitAsync()
    {
        if (OpenUnitDialogHandler != null)
        {
            var unit = await OpenUnitDialogHandler();
            if (unit != null)
            {
                var unitRes = await _unitApiService.GetAllAsync();
                if (unitRes.Success && unitRes.Data != null)
                {
                    Units = new ObservableCollection<UnitDto>(unitRes.Data);
                }
                else if (!Units.Any(u => u.Id == unit.Id))
                {
                    Units.Add(unit);
                }
                SelectedAddUnitId = unit.Id;
            }
        }
    }

    public ProductFormViewModel(
        IProductApiService productApiService,
        ICategoryApiService categoryApiService,
        IUnitApiService unitApiService,
        IProductImageApiService productImageApiService,
        IWarehouseApiService warehouseApiService)
    {
        _productApiService = productApiService;
        _categoryApiService = categoryApiService;
        _unitApiService = unitApiService;
        _productImageApiService = productImageApiService;
        _warehouseApiService = warehouseApiService;
    }

    public async Task InitializeAsync(ProductDto? p)
    {
        // 1. Load Categories, Units, Showrooms & Storage Warehouses (with full fallback to all active warehouses)
        var catRes = await _categoryApiService.GetAllAsync();
        if (catRes.Success && catRes.Data != null) Categories = new ObservableCollection<CategoryDto>(catRes.Data);

        var unitRes = await _unitApiService.GetAllAsync();
        if (unitRes.Success && unitRes.Data != null) Units = new ObservableCollection<UnitDto>(unitRes.Data);

        var allWhRes = await _warehouseApiService.GetAllAsync();
        var allWarehouses = (allWhRes.Success && allWhRes.Data != null) ? allWhRes.Data : new System.Collections.Generic.List<WarehouseSummaryDto>();

        var showRes = await _warehouseApiService.GetAllAsync(type: WarehouseType.Show);
        var showroomList = (showRes.Success && showRes.Data != null && showRes.Data.Count > 0) ? showRes.Data : allWarehouses;
        Showrooms = new ObservableCollection<WarehouseSummaryDto>(showroomList);
        SelectedShowroom = Showrooms.Count > 0 ? Showrooms[0] : null;

        ShowroomInitialQuantities = new ObservableCollection<CreateShowroomStockQuantityRequest>(
            Showrooms.Select(s => new CreateShowroomStockQuantityRequest
            {
                WarehouseId = s.Id,
                WarehouseName = s.Name,
                Quantity = 0
            })
        );

        var storgeRes = await _warehouseApiService.GetAllAsync(type: WarehouseType.Storge);
        var storgeList = (storgeRes.Success && storgeRes.Data != null && storgeRes.Data.Count > 0) ? storgeRes.Data : allWarehouses;
        StorageWarehouses = new ObservableCollection<WarehouseSummaryDto>(storgeList);
        SelectedStorageWarehouse = StorageWarehouses.Count > 0 ? StorageWarehouses[0] : null;

        SyncStorageQuantitiesMap();

        // 2. Set Product values
        if (p != null)
        {
            IsEditMode = true;
            ProductId = p.Id;
            Name = p.Name;
            Code = p.Code;
            Description = p.Description;
            CostPrice = p.CostPrice;
            SalePrice = p.SalePrice;
            CategoryId = p.CategoryId;

            // Populate Product Units
            ProductUnits = new ObservableCollection<CreateProductUnitRequest>(
                p.Units.Select(u => new CreateProductUnitRequest
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
                    ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                    IsDefault = u.IsDefault
                })
            );

            // Populate Product BarCodes
            ProductBarCodes = new ObservableCollection<CreateProductBarCodeRequest>(
                p.BarCodes.Select(b => new CreateProductBarCodeRequest
                {
                    BarCode = b.BarCode,
                    Title = string.IsNullOrWhiteSpace(b.Title) ? p.Name : b.Title,
                    Description = b.Description
                })
            );

            ExistingBarCodesForImages = new ObservableCollection<ProductBarCodeDto>(p.BarCodes);
            SyncBarcodesForImages();
            await LoadProductImagesAsync();
        }
        else
        {
            IsEditMode = false;
            ProductId = null;
            Name = string.Empty;
            Code = string.Empty;
            Description = string.Empty;
            CostPrice = 0;
            SalePrice = 0;
            CategoryId = Guid.Empty;
            ProductUnits = new ObservableCollection<CreateProductUnitRequest>();
            ProductBarCodes = new ObservableCollection<CreateProductBarCodeRequest>();
            ProductImages = new ObservableCollection<ProductImageDto>();
            PendingProductImages = new ObservableCollection<PendingProductImage>();
            ExistingBarCodesForImages = new ObservableCollection<ProductBarCodeDto>();
            SyncBarcodesForImages();
        }

        // Reset Add inputs
        SelectedAddUnitId = null;
        SelectedAddConversionFactor = 1;
        SelectedAddIsDefault = false;
        SelectedAddBarCode = string.Empty;
        SelectedAddBarCodeTitle = string.Empty;
        SelectedAddBarCodeDescription = string.Empty;
        SelectedUploadFilePath = string.Empty;
        SelectedUploadIsDefault = false;
        SelectedUploadBarcodeId = null;
    }

    private async Task LoadProductImagesAsync()
    {
        if (!ProductId.HasValue) return;
        var res = await _productImageApiService.GetByProductAsync(ProductId.Value);
        if (res.Success && res.Data != null)
        {
            foreach (var img in res.Data)
            {
                if (img.BarcodeId.HasValue)
                {
                    var bc = ExistingBarCodesForImages.FirstOrDefault(b => b.Id == img.BarcodeId.Value);
                    img.BarcodeTitle = bc != null
                        ? (string.IsNullOrWhiteSpace(bc.Title) ? bc.BarCode : $"{bc.Title} ({bc.BarCode})")
                        : "كود خاص";
                }
                else
                {
                    img.BarcodeTitle = "صورة عامة للمنتج";
                }
            }
            ProductImages = new ObservableCollection<ProductImageDto>(res.Data);
        }
    }

    [RelayCommand]
    private void BrowseImageFile()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "ملفات الصور (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|جميع الملفات (*.*)|*.*",
            Title = "اختر صورة للمنتج"
        };
        if (dialog.ShowDialog() == true)
        {
            SelectedUploadFilePath = dialog.FileName;
        }
    }

    [RelayCommand]
    private async Task UploadProductImageAsync()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(SelectedUploadFilePath) || !System.IO.File.Exists(SelectedUploadFilePath))
        {
            ErrorMessage = "يرجى اختيار صورة صالحة من جهازك عبر زر الاستعراض";
            return;
        }

        // منتج جديد لم يُحفظ بعد: تسجيل الصورة في قائمة الانتظار وعرضها في الشبكة،
        // وسيتم رفعها تلقائياً فور نجاح حفظ المنتج
        if (!ProductId.HasValue || ProductId.Value == Guid.Empty)
        {
            string barCode = string.Empty;
            string barcodeTitle = "صورة عامة للمنتج";
            if (SelectedUploadBarcodeId.HasValue)
            {
                var bc = ExistingBarCodesForImages.FirstOrDefault(b => b.Id == SelectedUploadBarcodeId.Value);
                if (bc != null)
                {
                    barCode = bc.BarCode;
                    barcodeTitle = string.IsNullOrWhiteSpace(bc.Title) ? bc.BarCode : bc.Title;
                }
            }

            if (PendingProductImages.Any(p => p.FilePath.Equals(SelectedUploadFilePath, StringComparison.OrdinalIgnoreCase) &&
                                              p.BarCode == barCode))
            {
                ErrorMessage = "هذه الصورة مضافة بالفعل لنفس الكود في قائمة الانتظار";
                return;
            }

            PendingProductImages.Add(new PendingProductImage
            {
                TempBarcodeId = SelectedUploadBarcodeId ?? Guid.Empty,
                BarCode = barCode,
                BarcodeTitle = barcodeTitle,
                FilePath = SelectedUploadFilePath,
                IsDefault = SelectedUploadIsDefault
            });

            SuccessMessage = "تمت إضافة الصورة لقائمة الانتظار وستُرفع تلقائياً فور حفظ المنتج.";
            SelectedUploadFilePath = string.Empty;
            SelectedUploadIsDefault = false;
            SelectedUploadBarcodeId = null;
            return;
        }

        // منتج محفوظ مسبقاً: رفع فوري مع متابعة التقدم
        await ExecuteAsync(async () =>
        {
            IsUploadingImage = true;
            UploadProgressPercent = 0;
            UploadStatusText = "جاري رفع الصورة ...";

            var progress = new Progress<int>(percent => { UploadProgressPercent = percent; });
            var res = await _productImageApiService.UploadAsync(
                ProductId.Value,
                SelectedUploadFilePath,
                SelectedUploadIsDefault,
                SelectedUploadBarcodeId,
                progress);

            if (res.Success)
            {
                SelectedUploadFilePath = string.Empty;
                SelectedUploadIsDefault = false;
                SelectedUploadBarcodeId = null;
                await LoadProductImagesAsync();
                SuccessMessage = "تم رفع الصورة بنجاح";
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل رفع الصورة";
            }

            IsUploadingImage = false;
            UploadStatusText = string.Empty;
        });
    }

    [RelayCommand]
    private void RemovePendingProductImage(PendingProductImage? pending)
    {
        if (pending != null) PendingProductImages.Remove(pending);
    }

    [RelayCommand]
    private async Task SetDefaultProductImageAsync(ProductImageDto? img)
    {
        if (img == null || !ProductId.HasValue) return;
        await ExecuteAsync(async () =>
        {
            var res = await _productImageApiService.SetDefaultAsync(ProductId.Value, img.Id);
            if (res.Success)
            {
                await LoadProductImagesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }

    [RelayCommand]
    private async Task RemoveProductImageAsync(ProductImageDto? img)
    {
        if (img == null || !ProductId.HasValue) return;
        await ExecuteAsync(async () =>
        {
            var res = await _productImageApiService.RemoveAsync(ProductId.Value, img.Id);
            if (res.Success)
            {
                await LoadProductImagesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }

    [RelayCommand]
    private void AddProductUnit()
    {
        ErrorMessage = string.Empty;
        if (!SelectedAddUnitId.HasValue || SelectedAddUnitId.Value == Guid.Empty)
        {
            ErrorMessage = "يرجى اختيار وحدة من القائمة";
            return;
        }

        if (SelectedAddConversionFactor <= 0)
        {
            ErrorMessage = "معامل التحويل (عدد القطع) يجب أن يكون أكبر من 0";
            return;
        }

        var unitItem = Units.FirstOrDefault(u => u.Id == SelectedAddUnitId.Value);
        string unitName = unitItem?.Name ?? "وحدة غير معروفة";

        if (ProductUnits.Any(u => u.UnitId == SelectedAddUnitId.Value))
        {
            ErrorMessage = "تمت إضافة هذه الوحدة من قبل";
            return;
        }

        ProductUnits.Add(new CreateProductUnitRequest
        {
            UnitId = SelectedAddUnitId.Value,
            UnitName = unitName,
            ConversionFactor = SelectedAddConversionFactor,
            IsDefault = SelectedAddIsDefault || ProductUnits.Count == 0
        });

        SelectedAddUnitId = null;
        SelectedAddConversionFactor = 1;
        SelectedAddIsDefault = false;
    }

    [RelayCommand]
    private void RemoveProductUnit(CreateProductUnitRequest? unit)
    {
        if (unit != null) ProductUnits.Remove(unit);
    }

    [RelayCommand]
    private void AddProductBarCode()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(SelectedAddBarCode))
        {
            ErrorMessage = "يرجى إدخال قيمة الباركود";
            return;
        }

        string title = string.IsNullOrWhiteSpace(SelectedAddBarCodeTitle) ? Name : SelectedAddBarCodeTitle.Trim();

        if (ProductBarCodes.Any(b => b.BarCode.Equals(SelectedAddBarCode.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            ErrorMessage = "تمت إضافة هذا الباركود من قبل";
            return;
        }

        string barCodeVal = SelectedAddBarCode.Trim();
        ProductBarCodes.Add(new CreateProductBarCodeRequest
        {
            BarCode = barCodeVal,
            Title = title,
            Description = SelectedAddBarCodeDescription?.Trim(),
            InitialQuantity = SelectedAddBarCodeInitialQuantity
        });

        SyncStorageQuantitiesMap();
        SyncBarcodesForImages();

        SelectedAddBarCode = string.Empty;
        SelectedAddBarCodeTitle = string.Empty;
        SelectedAddBarCodeDescription = string.Empty;
        SelectedAddBarCodeInitialQuantity = 0;
    }

    [RelayCommand]
    private void RemoveProductBarCode(CreateProductBarCodeRequest? barCode)
    {
        if (barCode != null)
        {
            ProductBarCodes.Remove(barCode);
            SyncStorageQuantitiesMap();
            SyncBarcodesForImages();
        }
    }

    private void SyncBarcodesForImages()
    {
        var currentBarcodes = ProductBarCodes.ToList();

        // Remove barcodes no longer present in ProductBarCodes
        var toRemove = ExistingBarCodesForImages
            .Where(e => !currentBarcodes.Any(b => b.BarCode.Equals(e.BarCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var rem in toRemove) ExistingBarCodesForImages.Remove(rem);

        // Add or update barcodes from ProductBarCodes
        foreach (var bc in currentBarcodes)
        {
            var titleText = string.IsNullOrWhiteSpace(bc.Title)
                ? bc.BarCode
                : $"{bc.Title} ({bc.BarCode})";

            var existing = ExistingBarCodesForImages.FirstOrDefault(e => e.BarCode.Equals(bc.BarCode, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                ExistingBarCodesForImages.Add(new ProductBarCodeDto
                {
                    Id = Guid.NewGuid(),
                    BarCode = bc.BarCode,
                    Title = titleText,
                    Description = bc.Description
                });
            }
            else
            {
                existing.Title = titleText;
            }
        }
    }

    private void SyncStorageQuantitiesMap()
    {
        if (StorageWarehouses == null || StorageWarehouses.Count == 0) return;

        var existingMap = StorageInitialQuantities != null
            ? StorageInitialQuantities.ToDictionary(s => $"{s.WarehouseId}_{s.BarCode.ToLower()}", s => s.Quantity)
            : new System.Collections.Generic.Dictionary<string, int>();

        var newList = new System.Collections.Generic.List<CreateStorageStockQuantityRequest>();
        foreach (var wh in StorageWarehouses)
        {
            foreach (var bc in ProductBarCodes)
            {
                if (string.IsNullOrWhiteSpace(bc.BarCode)) continue;

                string key = $"{wh.Id}_{bc.BarCode.Trim().ToLower()}";
                int initQty = existingMap.TryGetValue(key, out int q) ? q : bc.InitialQuantity;

                newList.Add(new CreateStorageStockQuantityRequest
                {
                    WarehouseId = wh.Id,
                    WarehouseName = wh.Name,
                    BarCode = bc.BarCode.Trim(),
                    Quantity = initQty
                });
            }
        }

        StorageInitialQuantities = new ObservableCollection<CreateStorageStockQuantityRequest>(newList);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) { ErrorMessage = "راجع الحقول المعلّمة قبل حفظ المنتج."; return; }
        if (string.IsNullOrWhiteSpace(Name) || CategoryId == Guid.Empty)
        {
            ErrorMessage = "يرجى تعبئة الحقول المطلوبة (اسم المنتج والتصنيف)";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && ProductId.HasValue)
            {
                var req = new UpdateProductRequest
                {
                    Name = Name,
                    Code = Code,
                    Description = Description,
                    CostPrice = CostPrice,
                    SalePrice = SalePrice,
                    CategoryId = CategoryId,
                    Units = ProductUnits.ToList(),
                    BarCodes = ProductBarCodes.ToList()
                };
                var res = await _productApiService.UpdateAsync(ProductId.Value, req);
                if (res.Success)
                {
                    CreatedOrUpdatedProduct = res.Data ?? new ProductDto
                    {
                        Id = ProductId.Value,
                        Name = Name,
                        Code = Code,
                        Description = Description,
                        CostPrice = CostPrice,
                        SalePrice = SalePrice,
                        CategoryId = CategoryId,
                        CategoryName = Categories.FirstOrDefault(c => c.Id == CategoryId)?.Name ?? string.Empty
                    };

                    // رفع الصور المعلقة المضافة أثناء التعديل (إن وجدت)
                    var createdBarcodes = res.Data?.BarCodes ?? ExistingBarCodesForImages.ToList();
                    bool allUploaded = await UploadPendingProductImagesAsync(ProductId.Value, createdBarcodes);
                    if (allUploaded)
                    {
                        CloseWindowHandler?.Invoke();
                    }
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new CreateProductRequest
                {
                    Name = Name,
                    Code = Code,
                    Description = Description,
                    CostPrice = CostPrice,
                    SalePrice = SalePrice,
                    CategoryId = CategoryId,
                    InitialShowroomQuantity = InitialShowroomQuantity,
                    ShowroomWarehouseId = SelectedShowroom?.Id,
                    StorageWarehouseId = SelectedStorageWarehouse?.Id,
                    ShowroomInitialQuantities = ShowroomInitialQuantities.Where(s => s.Quantity > 0).ToList(),
                    StorageInitialQuantities = StorageInitialQuantities.Where(s => s.Quantity > 0).ToList(),
                    Units = ProductUnits.ToList(),
                    BarCodes = ProductBarCodes.ToList()
                };
                var res = await _productApiService.CreateAsync(req);
                if (res.Success && res.Data != null)
                {
                    CreatedOrUpdatedProduct = res.Data;
                    ProductId = res.Data.Id;
                    IsEditMode = true;

                    // رفع جميع الصور المعلقة المرتبطة بالأكواد (يُعاد ربط
                    // معرف الباركود المؤقت بالمعرف الحقيقي الصادر من الخادم)
                    bool allUploaded = await UploadPendingProductImagesAsync(res.Data.Id, res.Data.BarCodes);

                    ExistingBarCodesForImages = new ObservableCollection<ProductBarCodeDto>(res.Data.BarCodes);

                    if (allUploaded)
                    {
                        CloseWindowHandler?.Invoke();
                    }
                    // عند فشل بعض الصور تبقى النافذة مفتوحة على وضع التعديل؛
                    // المنتج محفوظ والصور الفاشلة ما زالت في قائمة الانتظار لإعادة المحاولة
                }
                else
                {
                    ErrorMessage = res.Message;
                }
            }
        });
    }

    /// <summary>
    /// رفع جميع الصور المعلقة للمنتج بعد حفظه، مع إعادة ربط معرف الباركود المؤقت
    /// بالمعرف الحقيقي الصادر من الخادم، وإظهار تقدم الرفع الكلي.
    /// الصور المرفوعة بنجاح تُزال من قائمة الانتظار والفاشلة تبقى لإعادة المحاولة.
    /// </summary>
    private async Task<bool> UploadPendingProductImagesAsync(Guid productId, List<ProductBarCodeDto> createdBarcodes)
    {
        if (PendingProductImages.Count == 0) return true;

        IsUploadingImage = true;
        UploadStatusText = $"جاري رفع الصور (0/{PendingProductImages.Count}) ...";
        UploadProgressPercent = 0;

        var pendingList = PendingProductImages.ToList();
        int completedCount = 0;
        var uploadErrors = new List<string>();

        foreach (var pending in pendingList)
        {
            Guid? matchedBarcodeId = null;
            if (!string.IsNullOrWhiteSpace(pending.BarCode))
            {
                var createdBc = createdBarcodes.FirstOrDefault(
                    b => b.BarCode.Equals(pending.BarCode, StringComparison.OrdinalIgnoreCase));
                if (createdBc != null) matchedBarcodeId = createdBc.Id;
            }

            var perImageProgress = new Progress<int>(percent =>
            {
                int overall = ((completedCount * 100) + percent) / pendingList.Count;
                if (overall > 100) overall = 100;
                UploadProgressPercent = overall;
            });

            var uploadRes = await _productImageApiService.UploadAsync(
                productId,
                pending.FilePath,
                pending.IsDefault,
                matchedBarcodeId,
                perImageProgress);

            if (!uploadRes.Success)
            {
                uploadErrors.Add($"{pending.FileName}: {uploadRes.Message}");
            }
            else
            {
                PendingProductImages.Remove(pending);
            }

            completedCount++;
            UploadStatusText = $"جاري رفع الصور ({completedCount}/{pendingList.Count}) ...";
        }

        IsUploadingImage = false;
        UploadStatusText = string.Empty;
        UploadProgressPercent = 0;

        if (uploadErrors.Count > 0)
        {
            ErrorMessage = "تم حفظ المنتج لكن فشل رفع بعض الصور: " + string.Join(" | ", uploadErrors) + " — الصور الفاشلة ما زالت في قائمة الانتظار.";
            return false;
        }

        return true;
    }
}
