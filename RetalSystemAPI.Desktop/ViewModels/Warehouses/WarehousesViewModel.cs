using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Warehouses;

public partial class WarehousesViewModel : BaseViewModel
{
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private WarehouseType? _selectedType;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        CurrentPage = 1;
        _ = LoadWarehousesAsync();
    }

    partial void OnSelectedTypeChanged(WarehouseType? value)
    {
        CurrentPage = 1;
        _ = LoadWarehousesAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadWarehousesAsync();
    }

    public Func<WarehouseSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public WarehousesViewModel(IWarehouseApiService warehouseApiService, IBranchApiService branchApiService)
    {
        _warehouseApiService = warehouseApiService;
        _branchApiService = branchApiService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAsync();
        await LoadWarehousesAsync();
    }

    private async Task LoadBranchesAsync()
    {
        var res = await _branchApiService.GetAllAsync();
        if (res.Success && res.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(res.Data);
        }
    }

    [RelayCommand]
    public async Task LoadWarehousesAsync()
    {
        await ExecuteAsync(async () =>
        {
            Guid? branchId = SelectedBranch?.Id;
            var response = await _warehouseApiService.GetPagedAsync(CurrentPage, PageSize, branchId, SelectedType);
            if (response.Success && response.Data != null)
            {
                Warehouses = new ObservableCollection<WarehouseSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل المخازن وصالات العرض";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadWarehousesAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadWarehousesAsync();
        }
    }

    [RelayCommand]
    private async Task CreateWarehouseAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadWarehousesAsync();
        }
    }

    [RelayCommand]
    private async Task EditWarehouseAsync(object? parameter)
    {
        if (parameter is WarehouseSummaryDto warehouse && OpenDialogHandler != null)
        {
            await OpenDialogHandler(warehouse);
            await LoadWarehousesAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteWarehouseAsync(object? parameter)
    {
        if (parameter is not WarehouseSummaryDto warehouse) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المخزن", $"هل أنت تأكد من حذف '{warehouse.Name}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _warehouseApiService.DeleteAsync(warehouse.Id);
            if (res.Success) await LoadWarehousesAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(object? parameter)
    {
        if (parameter is not WarehouseSummaryDto warehouse) return;
        await ExecuteAsync(async () =>
        {
            var res = await _warehouseApiService.ToggleActiveAsync(warehouse.Id);
            if (res.Success) await LoadWarehousesAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class WarehouseFormViewModel : BaseViewModel
{
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private Guid? _warehouseId;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private WarehouseType _type = WarehouseType.Storge;

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public WarehouseFormViewModel(IWarehouseApiService warehouseApiService, IBranchApiService branchApiService)
    {
        _warehouseApiService = warehouseApiService;
        _branchApiService = branchApiService;
    }

    public async Task InitializeAsync(WarehouseSummaryDto? warehouse)
    {
        var res = await _branchApiService.GetAllAsync();
        if (res.Success && res.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(res.Data);
        }

        if (warehouse != null)
        {
            IsEditMode = true;
            WarehouseId = warehouse.Id;
            Name = warehouse.Name;
            Type = warehouse.Type;
            SelectedBranch = Branches.Count > 0 ? Branches.FirstOrDefault(b => b.Id == warehouse.BranchId) : null;
        }
        else
        {
            IsEditMode = false;
            WarehouseId = null;
            Name = string.Empty;
            Type = WarehouseType.Storge;
            SelectedBranch = Branches.Count > 0 ? Branches[0] : null;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "اسم المخزن أو الصالة مطلوب";
            return;
        }

        if (!IsEditMode && SelectedBranch == null)
        {
            ErrorMessage = "اختيار الفرع مطلوب";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && WarehouseId.HasValue)
            {
                var req = new UpdateWarehouseRequest
                {
                    Name = Name,
                    Type = Type
                };
                var res = await _warehouseApiService.UpdateAsync(WarehouseId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
            else if (SelectedBranch != null)
            {
                var req = new CreateWarehouseRequest
                {
                    BranchId = SelectedBranch.Id,
                    Name = Name,
                    Type = Type
                };
                var res = await _warehouseApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
        });
    }
}
