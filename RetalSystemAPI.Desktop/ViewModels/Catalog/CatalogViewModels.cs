using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Services.Catalog;
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
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private Guid? _parentCategoryId;
    [ObservableProperty] private ObservableCollection<CategoryDto> _availableParentCategories = new();
    [ObservableProperty] private bool _isEditMode;
    public Action? CloseWindowHandler { get; set; }

    public CategoryFormViewModel(ICategoryApiService categoryApiService)
    {
        _categoryApiService = categoryApiService;
    }

    public void Initialize(CategoryDto? cat)
    {
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
        if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "اسم التصنيف مطلوب"; return; }

        Guid? parentIdToSave = (ParentCategoryId.HasValue && ParentCategoryId.Value != Guid.Empty)
            ? ParentCategoryId.Value
            : null;

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && CategoryId.HasValue)
            {
                var res = await _categoryApiService.UpdateAsync(CategoryId.Value, new UpdateCategoryRequest { Name = Name, ParentCategoryId = parentIdToSave });
                if (res.Success) CloseWindowHandler?.Invoke(); else ErrorMessage = res.Message;
            }
            else
            {
                var res = await _categoryApiService.CreateAsync(new CreateCategoryRequest { Name = Name, ParentCategoryId = parentIdToSave });
                if (res.Success) CloseWindowHandler?.Invoke(); else ErrorMessage = res.Message;
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
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _symbol;
    [ObservableProperty] private bool _isEditMode;
    public Action? CloseWindowHandler { get; set; }

    public UnitFormViewModel(IUnitApiService unitApiService)
    {
        _unitApiService = unitApiService;
    }

    public void Initialize(UnitDto? unit)
    {
        if (unit != null) { IsEditMode = true; UnitId = unit.Id; Name = unit.Name; Symbol = unit.Symbol; }
        else { IsEditMode = false; UnitId = null; Name = string.Empty; Symbol = string.Empty; }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) { ErrorMessage = "اسم الوحدة مطلوب"; return; }
        await ExecuteAsync(async () =>
        {
            if (IsEditMode && UnitId.HasValue)
            {
                var res = await _unitApiService.UpdateAsync(UnitId.Value, new UpdateUnitRequest { Name = Name, Symbol = Symbol });
                if (res.Success) CloseWindowHandler?.Invoke(); else ErrorMessage = res.Message;
            }
            else
            {
                var res = await _unitApiService.CreateAsync(new CreateUnitRequest { Name = Name, Symbol = Symbol });
                if (res.Success) CloseWindowHandler?.Invoke(); else ErrorMessage = res.Message;
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

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchRes = await _productApiService.SearchAsync(SearchQuery);
                if (searchRes.Success && searchRes.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(searchRes.Data);
                    TotalPages = 1;
                }
            }
            else
            {
                var res = await _productApiService.GetPagedAsync(CurrentPage, PageSize);
                if (res.Success && res.Data != null)
                {
                    Products = new ObservableCollection<ProductDto>(res.Data.Items);
                    TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
                }
            }
        });
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

    [RelayCommand]
    private async Task ImportExcelAsync()
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

public partial class ProductFormViewModel : BaseViewModel
{
    private readonly IProductApiService _productApiService;
    private readonly ICategoryApiService _categoryApiService;
    private readonly IUnitApiService _unitApiService;
    private readonly IProductImageApiService _productImageApiService;

    [ObservableProperty] private Guid? _productId;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private string? _description;
    [ObservableProperty] private decimal _costPrice;
    [ObservableProperty] private decimal _salePrice;
    [ObservableProperty] private Guid _categoryId;

    [ObservableProperty] private ObservableCollection<CategoryDto> _categories = new();
    [ObservableProperty] private ObservableCollection<UnitDto> _units = new();

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
    [ObservableProperty] private ObservableCollection<ProductBarCodeDto> _existingBarCodesForImages = new();
    [ObservableProperty] private string _selectedUploadFilePath = string.Empty;
    [ObservableProperty] private bool _selectedUploadIsDefault;
    [ObservableProperty] private Guid? _selectedUploadBarcodeId;

    [ObservableProperty] private bool _isEditMode;
    public Action? CloseWindowHandler { get; set; }

    public ProductFormViewModel(
        IProductApiService productApiService,
        ICategoryApiService categoryApiService,
        IUnitApiService unitApiService,
        IProductImageApiService productImageApiService)
    {
        _productApiService = productApiService;
        _categoryApiService = categoryApiService;
        _unitApiService = unitApiService;
        _productImageApiService = productImageApiService;
    }

    public async Task InitializeAsync(ProductDto? p)
    {
        // 1. Load Categories & Units first so drop-downs are populated
        var catRes = await _categoryApiService.GetAllAsync();
        if (catRes.Success && catRes.Data != null) Categories = new ObservableCollection<CategoryDto>(catRes.Data);

        var unitRes = await _unitApiService.GetAllAsync();
        if (unitRes.Success && unitRes.Data != null) Units = new ObservableCollection<UnitDto>(unitRes.Data);

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
            ExistingBarCodesForImages = new ObservableCollection<ProductBarCodeDto>();
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
        if (!ProductId.HasValue || ProductId.Value == Guid.Empty)
        {
            ErrorMessage = "يرجى حفظ بيانات المنتج الأساسية أولاً لتفعيل إمكانية رفع الصور";
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedUploadFilePath) || !System.IO.File.Exists(SelectedUploadFilePath))
        {
            ErrorMessage = "يرجى اختيار صورة صالحة من جهازك عبر زر الاستعراض";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _productImageApiService.UploadAsync(
                ProductId.Value,
                SelectedUploadFilePath,
                SelectedUploadIsDefault,
                SelectedUploadBarcodeId);

            if (res.Success)
            {
                SelectedUploadFilePath = string.Empty;
                SelectedUploadIsDefault = false;
                SelectedUploadBarcodeId = null;
                await LoadProductImagesAsync();
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل رفع الصورة";
            }
        });
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

        ProductBarCodes.Add(new CreateProductBarCodeRequest
        {
            BarCode = SelectedAddBarCode.Trim(),
            Title = title,
            Description = SelectedAddBarCodeDescription?.Trim(),
            InitialQuantity = SelectedAddBarCodeInitialQuantity
        });

        SelectedAddBarCode = string.Empty;
        SelectedAddBarCodeTitle = string.Empty;
        SelectedAddBarCodeDescription = string.Empty;
        SelectedAddBarCodeInitialQuantity = 0;
    }

    [RelayCommand]
    private void RemoveProductBarCode(CreateProductBarCodeRequest? barCode)
    {
        if (barCode != null) ProductBarCodes.Remove(barCode);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
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
                if (res.Success) CloseWindowHandler?.Invoke(); else ErrorMessage = res.Message;
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
                    Units = ProductUnits.ToList(),
                    BarCodes = ProductBarCodes.ToList()
                };
                var res = await _productApiService.CreateAsync(req);
                if (res.Success && res.Data != null)
                {
                    // الانتقال لوضع التعديل مباشرة لتمكين رفع الصور فور الانتهاء من إنشاء المنتج
                    ProductId = res.Data.Id;
                    IsEditMode = true;
                    ExistingBarCodesForImages = new ObservableCollection<ProductBarCodeDto>(res.Data.BarCodes);
                    CloseWindowHandler?.Invoke();
                }
                else
                {
                    ErrorMessage = res.Message;
                }
            }
        });
    }
}
