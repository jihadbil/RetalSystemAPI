using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Suppliers;

public partial class SuppliersViewModel : BaseViewModel
{
    private readonly ISupplierApiService _supplierApiService;

    [ObservableProperty]
    private ObservableCollection<SupplierSummaryDto> _suppliers = new();

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _ = LoadSuppliersAsync();
    }

    partial void OnPageSizeChanged(int value)
    {
        CurrentPage = 1;
        _ = LoadSuppliersAsync();
    }

    public Func<SupplierSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public SuppliersViewModel(ISupplierApiService supplierApiService)
    {
        _supplierApiService = supplierApiService;
        _ = LoadSuppliersAsync();
    }

    [RelayCommand]
    public async Task LoadSuppliersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _supplierApiService.GetPagedAsync(CurrentPage, PageSize, SearchQuery);
            if (response.Success && response.Data != null)
            {
                Suppliers = new ObservableCollection<SupplierSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل الموردين";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    private async Task CreateSupplierAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    private async Task EditSupplierAsync(object? parameter)
    {
        if (parameter is SupplierSummaryDto supplier && OpenDialogHandler != null)
        {
            await OpenDialogHandler(supplier);
            await LoadSuppliersAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(object? parameter)
    {
        if (parameter is not SupplierSummaryDto supplier) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المورد", $"هل أنت متاكد من حذف المورد '{supplier.Name}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _supplierApiService.DeleteAsync(supplier.Id);
            if (res.Success) await LoadSuppliersAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class SupplierFormViewModel : BaseViewModel
{
    private readonly ISupplierApiService _supplierApiService;

    [ObservableProperty]
    private Guid? _supplierId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private decimal _openingBalance;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string? _newPhoneName;

    [ObservableProperty]
    private ObservableCollection<SupplierPhoneDto> _phones = new();

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public SupplierFormViewModel(ISupplierApiService supplierApiService)
    {
        _supplierApiService = supplierApiService;
    }

    public async Task InitializeAsync(Guid? id)
    {
        if (id.HasValue)
        {
            IsEditMode = true;
            SupplierId = id.Value;
            await LoadSupplierDetailsAsync(id.Value);
        }
        else
        {
            IsEditMode = false;
            SupplierId = null;
            Name = string.Empty;
            Address = string.Empty;
            OpeningBalance = 0;
            Phones = new ObservableCollection<SupplierPhoneDto>();
        }
    }

    private async Task LoadSupplierDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _supplierApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                Name = res.Data.Name;
                Address = res.Data.Address;
                OpeningBalance = res.Data.OpeningBalance;
                Phones = new ObservableCollection<SupplierPhoneDto>(res.Data.Phones);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل المورد";
            }
        });
    }

    [RelayCommand]
    private async Task AddPhoneAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            ErrorMessage = "رقم الهاتف مطلوب";
            return;
        }

        if (IsEditMode && SupplierId.HasValue)
        {
            await ExecuteAsync(async () =>
            {
                var req = new CreateSupplierPhoneRequest { PhoneNumber = NewPhoneNumber, Name = NewPhoneName };
                var res = await _supplierApiService.AddPhoneAsync(SupplierId.Value, req);
                if (res.Success && res.Data != null)
                {
                    Phones = new ObservableCollection<SupplierPhoneDto>(res.Data.Phones);
                    NewPhoneNumber = string.Empty;
                    NewPhoneName = string.Empty;
                }
                else ErrorMessage = res.Message;
            });
        }
        else
        {
            Phones.Add(new SupplierPhoneDto { Id = Guid.NewGuid(), PhoneNumber = NewPhoneNumber, Name = NewPhoneName });
            NewPhoneNumber = string.Empty;
            NewPhoneName = string.Empty;
        }
    }

    [RelayCommand]
    private async Task DeletePhoneAsync(object? parameter)
    {
        if (parameter is not SupplierPhoneDto phone) return;
        if (IsEditMode && SupplierId.HasValue)
        {
            await ExecuteAsync(async () =>
            {
                var res = await _supplierApiService.DeletePhoneAsync(SupplierId.Value, phone.Id);
                if (res.Success)
                {
                    Phones.Remove(phone);
                }
                else ErrorMessage = res.Message;
            });
        }
        else
        {
            Phones.Remove(phone);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "اسم المورد مطلوب";
            return;
        }

        // إذا أدخل المستخدم رقم هاتف في الحقل ولم يضغط على زر "+ إضافة"
        if (!string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            if (!Phones.Any(p => p.PhoneNumber == NewPhoneNumber))
            {
                Phones.Add(new SupplierPhoneDto { Id = Guid.NewGuid(), PhoneNumber = NewPhoneNumber, Name = NewPhoneName });
            }
            NewPhoneNumber = string.Empty;
            NewPhoneName = string.Empty;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && SupplierId.HasValue)
            {
                var req = new UpdateSupplierRequest
                {
                    Name = Name,
                    Address = Address,
                    OpeningBalance = OpeningBalance
                };
                var res = await _supplierApiService.UpdateAsync(SupplierId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل حفظ بيانات المورد";
            }
            else
            {
                var phoneRequests = Phones.Select(p => new CreateSupplierPhoneRequest { PhoneNumber = p.PhoneNumber, Name = p.Name }).ToList();
                var req = new CreateSupplierRequest
                {
                    Name = Name,
                    Address = Address,
                    OpeningBalance = OpeningBalance,
                    Phones = phoneRequests
                };
                var res = await _supplierApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل إضافة المورد";
            }
        });
    }
}
