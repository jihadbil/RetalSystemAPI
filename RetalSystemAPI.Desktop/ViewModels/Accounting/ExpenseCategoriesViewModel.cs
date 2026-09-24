using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Accounting;

public partial class ExpenseCategoriesViewModel : BaseViewModel
{
    private readonly IExpenseCategoryApiService _categoryApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<ExpenseCategoryDto> _categories = new();

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private bool? _selectedActiveStatus;

    public Func<ExpenseCategoryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    partial void OnSearchQueryChanged(string? value)
    {
        _ = DebounceSearchAsync(LoadCategoriesAsync);
    }

    partial void OnSelectedActiveStatusChanged(bool? value)
    {
        _ = LoadCategoriesAsync();
    }

    public ExpenseCategoriesViewModel(IExpenseCategoryApiService categoryApiService, IToastService toastService)
    {
        _categoryApiService = categoryApiService;
        _toastService = toastService;
        _ = LoadCategoriesAsync();
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _categoryApiService.GetAllAsync(SelectedActiveStatus, SearchQuery);
            if (res.Success && res.Data != null)
            {
                Categories = new ObservableCollection<ExpenseCategoryDto>(res.Data);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تصنيفات المصروفات";
            }
        });
    }

    [RelayCommand]
    private async Task CreateCategoryAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadCategoriesAsync();
        }
    }

    [RelayCommand]
    private async Task EditCategoryAsync(object? parameter)
    {
        if (parameter is ExpenseCategoryDto category && OpenDialogHandler != null)
        {
            await OpenDialogHandler(category);
            await LoadCategoriesAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(object? parameter)
    {
        if (parameter is not ExpenseCategoryDto category) return;
        await ExecuteAsync(async () =>
        {
            var res = await _categoryApiService.ToggleActiveStatusAsync(category.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم تعديل حالة التصنيف بنجاح");
                await LoadCategoriesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
                _toastService.ShowError(res.Message ?? "فشل تعديل حالة التصنيف");
            }
        });
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(object? parameter)
    {
        if (parameter is not ExpenseCategoryDto category) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد الحذف", $"هل أنت متأكد من حذف تصنيف المصروف '{category.Name}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _categoryApiService.DeleteAsync(category.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم حذف التصنيف بنجاح");
                await LoadCategoriesAsync();
            }
            else
            {
                ErrorMessage = res.Message;
                _toastService.ShowError(res.Message ?? "فشل حذف التصنيف");
            }
        });
    }
}

// ══════════════ EXPENSE CATEGORY FORM VIEWMODEL ══════════════

public partial class ExpenseCategoryFormViewModel : BaseViewModel
{
    private readonly IExpenseCategoryApiService _categoryApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private Guid? _categoryId;

    [ObservableProperty, NotifyDataErrorInfo]
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    private string _name = string.Empty;

    public bool IsEditMode => CategoryId.HasValue;
    public Action? CloseWindowHandler { get; set; }

    public ExpenseCategoryFormViewModel(IExpenseCategoryApiService categoryApiService, IToastService toastService)
    {
        _categoryApiService = categoryApiService;
        _toastService = toastService;
    }

    public void Initialize(ExpenseCategoryDto? category = null)
    {
        CategoryId = category?.Id;
        Name = category?.Name ?? string.Empty;
        OnPropertyChanged(nameof(IsEditMode));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        await ExecuteAsync(async () =>
        {
            if (IsEditMode)
            {
                var req = new UpdateExpenseCategoryRequest { Name = Name.Trim() };
                var res = await _categoryApiService.UpdateAsync(CategoryId!.Value, req);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم تحديث التصنيف بنجاح");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
            else
            {
                var req = new CreateExpenseCategoryRequest { Name = Name.Trim() };
                var res = await _categoryApiService.CreateAsync(req);
                if (res.Success)
                {
                    _toastService.ShowSuccess("تم إنشاء التصنيف بنجاح");
                    CloseWindowHandler?.Invoke();
                }
                else ErrorMessage = res.Message;
            }
        });
    }
}
