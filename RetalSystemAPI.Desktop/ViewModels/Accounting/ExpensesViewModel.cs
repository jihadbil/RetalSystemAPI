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

public partial class ExpensesViewModel : BaseViewModel
{
    private readonly IExpenseApiService _expenseApiService;
    private readonly IExpenseCategoryApiService _categoryApiService;
    private readonly ISafeApiService _safeApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<ExpenseSummaryDto> _expenses = new();

    [ObservableProperty]
    private ObservableCollection<ExpenseCategoryDto> _categories = new();

    [ObservableProperty]
    private ExpenseCategoryDto? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<SafeSummaryDto> _safes = new();

    [ObservableProperty]
    private SafeSummaryDto? _selectedSafe;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private bool? _selectedCancelledStatus;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private decimal _totalExpensesAmount;

    [ObservableProperty]
    private int _totalExpensesCount;

    private int _loadVersion;

    public Func<ExpenseSummaryDto?, Task>? OpenExpenseDialogHandler { get; set; }
    public Func<ExpenseSummaryDto, Task<string?>>? PromptCancelHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _loadVersion++;
        _ = DebounceSearchAsync(LoadExpensesAsync);
    }

    partial void OnSelectedCategoryChanged(ExpenseCategoryDto? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnSelectedSafeChanged(SafeSummaryDto? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnSelectedBranchChanged(BranchDto? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnFromDateChanged(DateTime? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnToDateChanged(DateTime? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnSelectedCancelledStatusChanged(bool? value) { CurrentPage = 1; _ = LoadExpensesAsync(); }
    partial void OnPageSizeChanged(int value) { CurrentPage = 1; _ = LoadExpensesAsync(); }

    public ExpensesViewModel(
        IExpenseApiService expenseApiService,
        IExpenseCategoryApiService categoryApiService,
        ISafeApiService safeApiService,
        IBranchApiService branchApiService,
        IToastService toastService)
    {
        _expenseApiService = expenseApiService;
        _categoryApiService = categoryApiService;
        _safeApiService = safeApiService;
        _branchApiService = branchApiService;
        _toastService = toastService;

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadFiltersAsync();
        await LoadExpensesAsync();
    }

    private async Task LoadFiltersAsync()
    {
        var catRes = await _categoryApiService.GetAllAsync(true);
        if (catRes.Success && catRes.Data != null)
        {
            var list = new List<ExpenseCategoryDto> { new() { Id = Guid.Empty, Name = "جميع التصنيفات" } };
            list.AddRange(catRes.Data);
            Categories = new ObservableCollection<ExpenseCategoryDto>(list);
            SelectedCategory = Categories.FirstOrDefault();
        }

        var branchesRes = await _branchApiService.GetAllAsync();
        if (branchesRes.Success && branchesRes.Data != null)
        {
            var list = new List<BranchDto> { new() { Id = Guid.Empty, Name = "جميع الفروع" } };
            list.AddRange(branchesRes.Data);
            Branches = new ObservableCollection<BranchDto>(list);
            SelectedBranch = Branches.FirstOrDefault();
        }

        var safesRes = await _safeApiService.GetAllAsync(null, null, true);
        if (safesRes.Success && safesRes.Data != null)
        {
            var list = new List<SafeSummaryDto> { new() { Id = Guid.Empty, Name = "جميع الخزن" } };
            list.AddRange(safesRes.Data);
            Safes = new ObservableCollection<SafeSummaryDto>(list);
            SelectedSafe = Safes.FirstOrDefault();
        }
    }

    [RelayCommand]
    public async Task LoadExpensesAsync()
    {
        var version = ++_loadVersion;
        await ExecuteAsync(async () =>
        {
            Guid? catId = SelectedCategory != null && SelectedCategory.Id != Guid.Empty ? SelectedCategory.Id : null;
            Guid? branchId = SelectedBranch != null && SelectedBranch.Id != Guid.Empty ? SelectedBranch.Id : null;
            Guid? safeId = SelectedSafe != null && SelectedSafe.Id != Guid.Empty ? SelectedSafe.Id : null;

            var res = await _expenseApiService.GetPagedAsync(
                CurrentPage, PageSize, catId, safeId, branchId, FromDate, ToDate, SelectedCancelledStatus, SearchQuery);

            if (version != _loadVersion) return;

            if (res.Success && res.Data != null)
            {
                Expenses = new ObservableCollection<ExpenseSummaryDto>(res.Data.Items);
                TotalPages = res.Data.TotalPages > 0 ? res.Data.TotalPages : 1;
                TotalExpensesCount = res.Data.TotalCount;
                TotalExpensesAmount = Expenses.Where(e => !e.IsCancelled).Sum(e => e.Amount);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل المصروفات التشغيلية";
            }
        }, isCurrent: () => version == _loadVersion);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadExpensesAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadExpensesAsync();
        }
    }

    [RelayCommand]
    private async Task CreateExpenseAsync()
    {
        if (OpenExpenseDialogHandler != null)
        {
            await OpenExpenseDialogHandler(null);
            await LoadExpensesAsync();
        }
    }

    [RelayCommand]
    private async Task EditExpenseAsync(object? parameter)
    {
        if (parameter is ExpenseSummaryDto expense && OpenExpenseDialogHandler != null)
        {
            await OpenExpenseDialogHandler(expense);
            await LoadExpensesAsync();
        }
    }

    [RelayCommand]
    private async Task CancelExpenseAsync(object? parameter)
    {
        if (parameter is not ExpenseSummaryDto expense) return;
        if (expense.IsCancelled)
        {
            _toastService.ShowInfo("هذا السند ملغى بالفعل");
            return;
        }

        if (PromptCancelHandler != null)
        {
            var reason = await PromptCancelHandler(expense);
            if (string.IsNullOrWhiteSpace(reason)) return;

            await ExecuteAsync(async () =>
            {
                var req = new CancelExpenseRequest { Reason = reason.Trim() };
                var res = await _expenseApiService.CancelAsync(expense.Id, req);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم إلغاء سند المصروف واسترجاع المبلغ للخزينة بنجاح");
                    await LoadExpensesAsync();
                }
                else
                {
                    ErrorMessage = res.Message;
                    _toastService.ShowError(res.Message ?? "فشل إلغاء سند المصروف");
                }
            });
        }
    }

    [RelayCommand]
    private async Task DeleteExpenseAsync(object? parameter)
    {
        if (parameter is not ExpenseSummaryDto expense) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد الحذف", $"هل أنت متأكد من حذف سند المصروف رقم '{expense.ExpenseNumber}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _expenseApiService.DeleteAsync(expense.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم حذف سند المصروف بنجاح");
                await LoadExpensesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
                _toastService.ShowError(res.Message ?? "فشل حذف سند المصروف");
            }
        });
    }
}

// ══════════════ EXPENSE FORM VIEWMODEL ══════════════

public partial class ExpenseFormViewModel : BaseViewModel
{
    private readonly IExpenseApiService _expenseApiService;
    private readonly IExpenseCategoryApiService _categoryApiService;
    private readonly ISafeApiService _safeApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private Guid? _expenseId;

    [ObservableProperty]
    private string _expenseNumber = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ExpenseCategoryDto> _categories = new();

    [ObservableProperty]
    private ExpenseCategoryDto? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<SafeSummaryDto> _safes = new();

    [ObservableProperty]
    private SafeSummaryDto? _selectedSafe;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
    private decimal _amount;

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private DateTime _expenseDate = DateTime.Today;

    [ObservableProperty]
    private string? _beneficiaryName;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "البيان وتفاصيل المصروف مطلوبة")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    private string _description = string.Empty;

    [ObservableProperty]
    private string? _notes;

    public bool IsEditMode => ExpenseId.HasValue;
    public Action? CloseWindowHandler { get; set; }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        _ = LoadSafesForBranchAsync(value?.Id);
    }

    public ExpenseFormViewModel(
        IExpenseApiService expenseApiService,
        IExpenseCategoryApiService categoryApiService,
        ISafeApiService safeApiService,
        IBranchApiService branchApiService,
        IToastService toastService)
    {
        _expenseApiService = expenseApiService;
        _categoryApiService = categoryApiService;
        _safeApiService = safeApiService;
        _branchApiService = branchApiService;
        _toastService = toastService;
    }

    private async Task LoadSafesForBranchAsync(Guid? branchId)
    {
        var res = await _safeApiService.GetAllAsync(branchId, null, true);
        if (res.Success && res.Data != null)
        {
            Safes = new ObservableCollection<SafeSummaryDto>(res.Data);
            SelectedSafe = Safes.FirstOrDefault();
        }
        else
        {
            Safes.Clear();
            SelectedSafe = null;
        }
    }

    public async Task InitializeAsync(Guid? expenseId = null)
    {
        ExpenseId = expenseId;

        // Load Categories
        var catRes = await _categoryApiService.GetAllAsync(true);
        if (catRes.Success && catRes.Data != null)
        {
            Categories = new ObservableCollection<ExpenseCategoryDto>(catRes.Data);
            SelectedCategory = Categories.FirstOrDefault();
        }

        // Load Branches
        var branchRes = await _branchApiService.GetAllAsync();
        if (branchRes.Success && branchRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchRes.Data);
            SelectedBranch = Branches.FirstOrDefault();
        }

        if (expenseId.HasValue)
        {
            var res = await _expenseApiService.GetByIdAsync(expenseId.Value);
            if (res.Success && res.Data != null)
            {
                ExpenseNumber = res.Data.ExpenseNumber;
                Amount = res.Data.Amount;
                PaymentMethod = res.Data.PaymentMethod;
                ExpenseDate = res.Data.ExpenseDate;
                BeneficiaryName = res.Data.BeneficiaryName;
                Description = res.Data.Description;
                Notes = res.Data.Notes;

                SelectedCategory = Categories.FirstOrDefault(c => c.Id == res.Data.ExpenseCategoryId);
                SelectedBranch = Branches.FirstOrDefault(b => b.Id == res.Data.BranchId);

                await LoadSafesForBranchAsync(res.Data.BranchId);
                if (res.Data.SafeId.HasValue)
                {
                    SelectedSafe = Safes.FirstOrDefault(s => s.Id == res.Data.SafeId.Value);
                }
            }
        }
        else
        {
            ExpenseNumber = string.Empty;
            Amount = 0;
            PaymentMethod = PaymentMethod.Cash;
            ExpenseDate = DateTime.Today;
            BeneficiaryName = null;
            Description = string.Empty;
            Notes = null;

            if (SelectedBranch != null)
            {
                await LoadSafesForBranchAsync(SelectedBranch.Id);
            }
        }

        OnPropertyChanged(nameof(IsEditMode));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (SelectedCategory == null)
        {
            ErrorMessage = "يجب اختيار تصنيف المصروف";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب اختيار الفرع";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode)
            {
                var req = new UpdateExpenseRequest
                {
                    ExpenseCategoryId = SelectedCategory.Id,
                    BeneficiaryName = BeneficiaryName?.Trim(),
                    Description = Description.Trim(),
                    Notes = Notes?.Trim()
                };
                var res = await _expenseApiService.UpdateAsync(ExpenseId!.Value, req);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم تحديث سند المصروف بنجاح");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new CreateExpenseRequest
                {
                    ExpenseCategoryId = SelectedCategory.Id,
                    BranchId = SelectedBranch.Id,
                    SafeId = SelectedSafe?.Id,
                    Amount = Amount,
                    PaymentMethod = PaymentMethod,
                    ExpenseDate = ExpenseDate,
                    BeneficiaryName = BeneficiaryName?.Trim(),
                    Description = Description.Trim(),
                    Notes = Notes?.Trim()
                };
                var res = await _expenseApiService.CreateAsync(req);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم تسجيل سند المصروف بنجاح وخصمه من الخزينة");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
        });
    }
}
