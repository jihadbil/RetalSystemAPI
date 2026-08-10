using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Branches;

public partial class BranchesViewModel : BaseViewModel
{
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadBranchesAsync();
    }

    public Func<BranchDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public BranchesViewModel(IBranchApiService branchApiService)
    {
        _branchApiService = branchApiService;
        _ = LoadBranchesAsync();
    }

    [RelayCommand]
    public async Task LoadBranchesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _branchApiService.GetPagedAsync(CurrentPage, PageSize);
            if (response.Success && response.Data != null)
            {
                Branches = new ObservableCollection<BranchDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else ErrorMessage = response.Message ?? "فشل تحميل الفروع";
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages) { CurrentPage++; await LoadBranchesAsync(); }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1) { CurrentPage--; await LoadBranchesAsync(); }
    }

    [RelayCommand]
    private async Task CreateBranchAsync()
    {
        if (OpenDialogHandler != null) { await OpenDialogHandler(null); await LoadBranchesAsync(); }
    }

    [RelayCommand]
    private async Task EditBranchAsync(BranchDto? branch)
    {
        if (branch != null && OpenDialogHandler != null) { await OpenDialogHandler(branch); await LoadBranchesAsync(); }
    }

    [RelayCommand]
    private async Task DeleteBranchAsync(BranchDto? branch)
    {
        if (branch == null) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف الفرع", $"هل أنت تأكد من حذف الفرع '{branch.Name}'؟ لا يمكن التراجع بعد الحذف.");
            if (!confirmed) return;
        }
        await ExecuteAsync(async () =>
        {
            var res = await _branchApiService.DeleteAsync(branch.Id);
            if (res.Success) await LoadBranchesAsync();
            else ErrorMessage = res.Message;
        });
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(BranchDto? branch)
    {
        if (branch == null) return;
        await ExecuteAsync(async () =>
        {
            var res = await _branchApiService.ToggleActiveAsync(branch.Id);
            if (res.Success) await LoadBranchesAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class BranchFormViewModel : BaseViewModel
{
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private Guid? _branchId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public BranchFormViewModel(IBranchApiService branchApiService)
    {
        _branchApiService = branchApiService;
    }

    public void Initialize(BranchDto? branch)
    {
        if (branch != null)
        {
            IsEditMode = true;
            BranchId = branch.Id;
            Name = branch.Name;
            Address = branch.Address;
            PhoneNumber = branch.PhoneNumbers.Count > 0 ? branch.PhoneNumbers[0] : string.Empty;
        }
        else
        {
            IsEditMode = false;
            BranchId = null;
            Name = string.Empty;
            Address = string.Empty;
            PhoneNumber = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "اسم الفرع مطلوب";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var phones = string.IsNullOrWhiteSpace(PhoneNumber) ? new System.Collections.Generic.List<string>() : new System.Collections.Generic.List<string> { PhoneNumber };
            if (IsEditMode && BranchId.HasValue)
            {
                var req = new UpdateBranchRequest { Name = Name, Address = Address, PhoneNumbers = phones };
                var res = await _branchApiService.UpdateAsync(BranchId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new CreateBranchRequest { Name = Name, Address = Address, PhoneNumbers = phones };
                var res = await _branchApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message;
            }
        });
    }
}
