using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Accounting;

public partial class SafesViewModel : BaseViewModel
{
    private readonly ISafeApiService _safeApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<SafeSummaryDto> _safes = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private SafeType? _selectedType;

    [ObservableProperty]
    private bool? _selectedActiveStatus;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private decimal _totalLiquidity;

    [ObservableProperty]
    private int _coffersCount;

    [ObservableProperty]
    private int _banksCount;

    [ObservableProperty]
    private int _drawersCount;

    private int _loadVersion;

    public Func<SafeSummaryDto?, Task>? OpenSafeDialogHandler { get; set; }
    public Func<SafeSummaryDto?, Task>? OpenTransferDialogHandler { get; set; }
    public Func<SafeSummaryDto?, Task>? OpenTransactionDialogHandler { get; set; }
    public Func<SafeSummaryDto, Task>? OpenLedgerHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadSafesAsync);
    }

    partial void OnSelectedBranchChanged(BranchDto? value) { CurrentPage = 1; _ = LoadSafesAsync(); }
    partial void OnSelectedTypeChanged(SafeType? value) { CurrentPage = 1; _ = LoadSafesAsync(); }
    partial void OnSelectedActiveStatusChanged(bool? value) { CurrentPage = 1; _ = LoadSafesAsync(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; _ = LoadSafesAsync(); }

    public SafesViewModel(ISafeApiService safeApiService, IBranchApiService branchApiService, IToastService toastService)
    {
        _safeApiService = safeApiService;
        _branchApiService = branchApiService;
        _toastService = toastService;
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadBranchesAsync();
        await LoadSafesAsync();
    }

    public async Task LoadBranchesAsync()
    {
        var res = await _branchApiService.GetAllAsync();
        if (res.Success && res.Data != null)
        {
            var list = new List<BranchDto> { new() { Id = Guid.Empty, Name = "جميع الفروع" } };
            list.AddRange(res.Data);
            Branches = new ObservableCollection<BranchDto>(list);
            SelectedBranch = Branches.FirstOrDefault();
        }
    }

    [RelayCommand]
    public async Task LoadSafesAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            Guid? branchId = SelectedBranch != null && SelectedBranch.Id != Guid.Empty ? SelectedBranch.Id : null;
            var response = await _safeApiService.GetPagedAsync(CurrentPage, PageSize, branchId, SelectedType, SelectedActiveStatus, SearchQuery);
            if (version != _loadVersion) return;

            if (response.Success && response.Data != null)
            {
                Safes = new ObservableCollection<SafeSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                    await LoadSafesAsync();
                    return;
                }

                // حساب إحصائيات السيولة
                var allSafesRes = await _safeApiService.GetAllAsync(branchId, null, true, null);
                if (allSafesRes.Success && allSafesRes.Data != null)
                {
                    TotalLiquidity = allSafesRes.Data.Sum(s => s.CurrentBalance);
                    CoffersCount = allSafesRes.Data.Count(s => s.Type == SafeType.CashBox);
                    BanksCount = allSafesRes.Data.Count(s => s.Type == SafeType.Bank);
                    DrawersCount = allSafesRes.Data.Count(s => s.Type == SafeType.CashDrawer);
                }
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل قائمة الخزن";
            }
        }, isCurrent: () => version == _loadVersion);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task CreateSafeAsync()
    {
        if (OpenSafeDialogHandler != null)
        {
            await OpenSafeDialogHandler(null);
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task EditSafeAsync(object? parameter)
    {
        if (parameter is SafeSummaryDto safe && OpenSafeDialogHandler != null)
        {
            await OpenSafeDialogHandler(safe);
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(object? parameter)
    {
        if (parameter is not SafeSummaryDto safe) return;
        await ExecuteAsync(async () =>
        {
            var res = await _safeApiService.ToggleActiveStatusAsync(safe.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess(res.Message ?? "تم تعديل حالة الخزينة بنجاح");
                await LoadSafesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
                _toastService.ShowError(res.Message ?? "فشل تعديل حالة الخزينة");
            }
        });
    }

    [RelayCommand]
    private async Task DeleteSafeAsync(object? parameter)
    {
        if (parameter is not SafeSummaryDto safe) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد الحذف", $"هل أنت متأكد من رغبتك في حذف الخزينة '{safe.Name}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _safeApiService.DeleteAsync(safe.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم حذف الخزينة بنجاح");
                await LoadSafesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
                _toastService.ShowError(res.Message ?? "فشل حذف الخزينة");
            }
        });
    }

    [RelayCommand]
    private async Task TransferFundsAsync(object? parameter)
    {
        var safe = parameter as SafeSummaryDto;
        if (OpenTransferDialogHandler != null)
        {
            await OpenTransferDialogHandler(safe);
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task DepositWithdrawAsync(object? parameter)
    {
        var safe = parameter as SafeSummaryDto;
        if (OpenTransactionDialogHandler != null)
        {
            await OpenTransactionDialogHandler(safe);
            await LoadSafesAsync();
        }
    }

    [RelayCommand]
    private async Task ViewLedgerAsync(object? parameter)
    {
        if (parameter is SafeSummaryDto safe && OpenLedgerHandler != null)
        {
            await OpenLedgerHandler(safe);
        }
    }
}

// ══════════════ SAFE FORM VIEWMODEL ══════════════

public partial class SafeFormViewModel : BaseViewModel
{
    private readonly ISafeApiService _safeApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private Guid? _safeId;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "اسم الخزينة مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    private string _name = string.Empty;

    [ObservableProperty]
    private SafeType _type = SafeType.CashBox;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي لا يمكن أن يكون سالباً")]
    private decimal _openingBalance;

    [ObservableProperty]
    private string? _bankAccountNumber;

    [ObservableProperty]
    private string? _bankName;

    [ObservableProperty]
    private string? _notes;

    public bool IsEditMode => SafeId.HasValue;
    public bool IsBankType => Type == SafeType.Bank;

    public Action? CloseWindowHandler { get; set; }

    partial void OnTypeChanged(SafeType value)
    {
        OnPropertyChanged(nameof(IsBankType));
    }

    public SafeFormViewModel(ISafeApiService safeApiService, IBranchApiService branchApiService, IToastService toastService)
    {
        _safeApiService = safeApiService;
        _branchApiService = branchApiService;
        _toastService = toastService;
    }

    public async Task InitializeAsync(Guid? id = null)
    {
        SafeId = id;
        var branchesRes = await _branchApiService.GetAllAsync();
        if (branchesRes.Success && branchesRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchesRes.Data);
            SelectedBranch = Branches.FirstOrDefault();
        }

        if (id.HasValue)
        {
            var res = await _safeApiService.GetByIdAsync(id.Value);
            if (res.Success && res.Data != null)
            {
                Name = res.Data.Name;
                Type = res.Data.Type;
                OpeningBalance = res.Data.OpeningBalance;
                BankAccountNumber = res.Data.BankAccountNumber;
                BankName = res.Data.BankName;
                Notes = res.Data.Notes;
                IsActive = res.Data.IsActive;
                SelectedBranch = Branches.FirstOrDefault(b => b.Id == res.Data.BranchId);
            }
        }
        else
        {
            Name = string.Empty;
            Type = SafeType.CashBox;
            IsActive = true;
            OpeningBalance = 0;
            BankAccountNumber = null;
            BankName = null;
            Notes = null;
        }

        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsBankType));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب اختيار الفرع التابعة له الخزينة";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode)
            {
                var updateReq = new UpdateSafeRequest
                {
                    Name = Name.Trim(),
                    Type = Type,
                    IsActive = IsActive,
                    BankAccountNumber = BankAccountNumber?.Trim(),
                    BankName = BankName?.Trim(),
                    Notes = Notes?.Trim()
                };
                var res = await _safeApiService.UpdateAsync(SafeId!.Value, updateReq);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم تحديث بيانات الخزينة بنجاح");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var createReq = new CreateSafeRequest
                {
                    Name = Name.Trim(),
                    Type = Type,
                    BranchId = SelectedBranch.Id,
                    OpeningBalance = OpeningBalance,
                    BankAccountNumber = BankAccountNumber?.Trim(),
                    BankName = BankName?.Trim(),
                    Notes = Notes?.Trim()
                };
                var res = await _safeApiService.CreateAsync(createReq);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم إنشاء الخزينة بنجاح");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
        });
    }
}

// ══════════════ SAFE TRANSFER VIEWMODEL ══════════════

public partial class SafeTransferViewModel : BaseViewModel
{
    private readonly ISafeApiService _safeApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<SafeSummaryDto> _availableSafes = new();

    [ObservableProperty]
    private SafeSummaryDto? _fromSafe;

    [ObservableProperty]
    private SafeSummaryDto? _toSafe;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
    private decimal _amount;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _notes;

    public Action? CloseWindowHandler { get; set; }

    public SafeTransferViewModel(ISafeApiService safeApiService, IToastService toastService)
    {
        _safeApiService = safeApiService;
        _toastService = toastService;
    }

    public async Task InitializeAsync(Guid? defaultFromSafeId = null)
    {
        var res = await _safeApiService.GetAllAsync(null, null, true, null);
        if (res.Success && res.Data != null)
        {
            AvailableSafes = new ObservableCollection<SafeSummaryDto>(res.Data);
            if (defaultFromSafeId.HasValue)
            {
                FromSafe = AvailableSafes.FirstOrDefault(s => s.Id == defaultFromSafeId.Value);
            }
            else
            {
                FromSafe = AvailableSafes.FirstOrDefault();
            }

            ToSafe = AvailableSafes.FirstOrDefault(s => s.Id != FromSafe?.Id);
        }
        Amount = 0;
        Description = "تحويل أموال بين الخزن";
        Notes = null;
    }

    [RelayCommand]
    private async Task TransferAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (FromSafe == null || ToSafe == null)
        {
            ErrorMessage = "يجب اختيار الخزينة المصدر والخزينة الوجهة";
            return;
        }

        if (FromSafe.Id == ToSafe.Id)
        {
            ErrorMessage = "لا يمكن التحويل لنفس الخزينة، يجب اختيار خزينتين مختلفتين";
            return;
        }

        if (Amount > FromSafe.CurrentBalance)
        {
            ErrorMessage = $"رصيد الخزينة المصدر ({FromSafe.CurrentBalance:N2}) غير كافٍ لإجراء تحويل بقيمة ({Amount:N2})";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new TransferFundsRequest
            {
                FromSafeId = FromSafe.Id,
                ToSafeId = ToSafe.Id,
                Amount = Amount,
                Description = Description?.Trim(),
                Notes = Notes?.Trim()
            };
            var res = await _safeApiService.TransferFundsAsync(req);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم تحويل الأموال بنجاح وخصمها من المصدر وإيداعها في الوجهة");
                CloseWindowHandler?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }
}

// ══════════════ SAFE TRANSACTION FORM VIEWMODEL ══════════════

public partial class SafeTransactionFormViewModel : BaseViewModel
{
    private readonly ISafeApiService _safeApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private SafeSummaryDto? _safe;

    [ObservableProperty]
    private TransactionDirection _direction = TransactionDirection.Inbound;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
    private decimal _amount;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "بيان الحركة مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    private string _description = string.Empty;

    [ObservableProperty]
    private string? _notes;

    public Action? CloseWindowHandler { get; set; }

    public SafeTransactionFormViewModel(ISafeApiService safeApiService, IToastService toastService)
    {
        _safeApiService = safeApiService;
        _toastService = toastService;
    }

    public void Initialize(SafeSummaryDto safe, TransactionDirection direction = TransactionDirection.Inbound)
    {
        Safe = safe;
        Direction = direction;
        Amount = 0;
        Description = direction == TransactionDirection.Inbound ? "إيداع نقدي يدوي" : "سحب نقدي يدوي";
        Notes = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (Safe == null)
        {
            ErrorMessage = "الخزينة غير محددة";
            return;
        }

        if (Direction == TransactionDirection.Outbound && Amount > Safe.CurrentBalance)
        {
            ErrorMessage = $"رصيد الخزينة الحالي ({Safe.CurrentBalance:N2}) لا يكفي لسحب مبلغ ({Amount:N2})";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreateSafeTransactionRequest
            {
                SafeId = Safe.Id,
                Direction = Direction,
                Amount = Amount,
                Description = Description.Trim(),
                Notes = Notes?.Trim()
            };
            var res = await _safeApiService.CreateTransactionAsync(req);
            if (res.Success)
            {
                _toastService.ShowSuccess(Direction == TransactionDirection.Inbound ? "تم تنفيذ حركة الإيداع بنجاح" : "تم تنفيذ حركة السحب بنجاح");
                CloseWindowHandler?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }
}

// ══════════════ SAFE LEDGER VIEWMODEL ══════════════

public partial class SafeLedgerViewModel : BaseViewModel
{
    private readonly ISafeApiService _safeApiService;

    [ObservableProperty]
    private Guid _safeId;

    [ObservableProperty]
    private string _safeName = string.Empty;

    [ObservableProperty]
    private decimal _currentBalance;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private TransactionDirection? _selectedDirection;

    [ObservableProperty]
    private ObservableCollection<SafeTransactionDto> _transactions = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    public SafeLedgerViewModel(ISafeApiService safeApiService)
    {
        _safeApiService = safeApiService;
    }

    public async Task InitializeAsync(Guid safeId, string safeName, decimal currentBalance)
    {
        SafeId = safeId;
        SafeName = safeName;
        CurrentBalance = currentBalance;
        CurrentPage = 1;
        FromDate = DateTime.Today.AddDays(-30);
        ToDate = DateTime.Today;
        SelectedDirection = null;

        await LoadTransactionsAsync();
    }

    [RelayCommand]
    public async Task LoadTransactionsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _safeApiService.GetTransactionsPagedAsync(SafeId, CurrentPage, PageSize, FromDate, ToDate, SelectedDirection, null);
            if (res.Success && res.Data != null)
            {
                Transactions = new ObservableCollection<SafeTransactionDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل حركات الخزينة";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadTransactionsAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadTransactionsAsync();
        }
    }
}
