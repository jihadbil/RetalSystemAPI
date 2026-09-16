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

    // القائمتان المفلترتان حسب النوع، والكومبو يعرض المناسب للتبويب الحالي عبر FilteredWarehouses
    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _storgeWarehouses = new();

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _showroomWarehouses = new();

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

    /// <summary>مطابقة الباركود تماماً — يقتصر البحث على التطابق التام مع الباركود بدلاً من الاحتواء</summary>
    [ObservableProperty] private bool _exactBarcodeMatch = false;

    partial void OnExactBarcodeMatchChanged(bool value)
    {
        PageNumber = 1;
        _ = RefreshCurrentTabAsync();
    }

    /// <summary>
    /// مصدر الكومبو الحالي حسب التبويب: تبويب المخزن يعرض المخازن فقط
    /// وتبويب الصالة يعرض الصالات فقط (تبويب التنبيهات يعرض المخازن).
    /// </summary>
    public ObservableCollection<WarehouseSummaryDto> FilteredWarehouses =>
        SelectedTabIndex == 1 ? ShowroomWarehouses : StorgeWarehouses;

    partial void OnStorgeWarehousesChanged(ObservableCollection<WarehouseSummaryDto> value)
        => OnPropertyChanged(nameof(FilteredWarehouses));

    partial void OnShowroomWarehousesChanged(ObservableCollection<WarehouseSummaryDto> value)
        => OnPropertyChanged(nameof(FilteredWarehouses));

    partial void OnSelectedWarehouseChanged(WarehouseSummaryDto? value)
    {
        PageNumber = 1;
        _ = RefreshCurrentTabAsync();
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        PageNumber = 1;
        // تبديل مصدر الكومبو حسب التبويب مع اختيار أول عنصر مناسب؛
        // تغيّر المحدد يُطلق التحديث تلقائياً، وإلا نحدّث يدوياً
        OnPropertyChanged(nameof(FilteredWarehouses));
        var first = FilteredWarehouses.FirstOrDefault();
        if (!ReferenceEquals(SelectedWarehouse, first))
        {
            SelectedWarehouse = first;
        }
        else
        {
            _ = RefreshCurrentTabAsync();
        }
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

    /// <summary>
    /// إعادة تحميل قائمة المخازن/الصالات عند كل ظهور للشاشة،
    /// بحيث تظهر المخازن والصالات المضافة حديثاً دون الحاجة لإعادة تشغيل التطبيق.
    /// يُحافظ على المستودع المحدد إن كان لا يزال موجوداً بنفس النوع.
    /// </summary>
    public async Task RefreshOnNavigatedAsync()
    {
        var previouslySelectedId = SelectedWarehouse?.Id;
        await LoadWarehousesAsync(previouslySelectedId);
        await RefreshCurrentTabAsync();
    }

    private async Task LoadWarehousesAsync(Guid? preferredWarehouseId = null)
    {
        // تحميل قائمتين مفلترتين: مخازن التخزين وصالات العرض
        var storgeRes = await _warehouseApiService.GetAllAsync(type: WarehouseType.Storge);
        if (storgeRes.Success && storgeRes.Data != null)
        {
            StorgeWarehouses = new ObservableCollection<WarehouseSummaryDto>(storgeRes.Data);
        }

        var showRes = await _warehouseApiService.GetAllAsync(type: WarehouseType.Show);
        if (showRes.Success && showRes.Data != null)
        {
            ShowroomWarehouses = new ObservableCollection<WarehouseSummaryDto>(showRes.Data);
        }

        // إن لم تتوفر قوائم مفلترة (بيانات قديمة بلا نوع) نرجع لكل المخازن
        if (StorgeWarehouses.Count == 0 && ShowroomWarehouses.Count == 0)
        {
            var allRes = await _warehouseApiService.GetAllAsync();
            if (allRes.Success && allRes.Data != null)
            {
                StorgeWarehouses = new ObservableCollection<WarehouseSummaryDto>(allRes.Data);
                if (StorgeWarehouses.Count > 0) SelectedWarehouse = StorgeWarehouses[0];
                return;
            }
        }


        var preferred = preferredWarehouseId.HasValue
            ? FilteredWarehouses.FirstOrDefault(w => w.Id == preferredWarehouseId.Value)
            : null;

        // الحفاظ على المحدد الحالي إن كان ما يزال ضمن قائمة التبويب الحالي
        if (SelectedWarehouse != null && FilteredWarehouses.Any(w => w.Id == SelectedWarehouse.Id))
        {
            return;
        }

        SelectedWarehouse = preferred ?? FilteredWarehouses.FirstOrDefault();
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
                    var res = await _stockApiService.GetPagedStorgeStocksByWarehouseAsync(SelectedWarehouse.Id, PageNumber, PageSize, SearchTerm, ExactBarcodeMatch);
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
                    var res = await _stockApiService.GetPagedShowroomStocksByWarehouseAsync(SelectedWarehouse.Id, PageNumber, PageSize, SearchTerm, ExactBarcodeMatch);
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
