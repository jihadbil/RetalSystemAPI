using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Customers;

public partial class CustomersViewModel : BaseViewModel
{
    private readonly ICustomerApiService _customerApiService;

    [ObservableProperty]
    private ObservableCollection<CustomerSummaryDto> _customers = new();

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private CustomerType? _selectedType;

    [ObservableProperty]
    private bool? _selectedActiveStatus;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _ = LoadCustomersAsync();
    }

    partial void OnSelectedTypeChanged(CustomerType? value)
    {
        CurrentPage = 1;
        _ = LoadCustomersAsync();
    }

    partial void OnSelectedActiveStatusChanged(bool? value)
    {
        CurrentPage = 1;
        _ = LoadCustomersAsync();
    }

    public Func<CustomerSummaryDto?, Task>? OpenDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public CustomersViewModel(ICustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    public async Task LoadCustomersAsync()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _customerApiService.GetPagedAsync(CurrentPage, PageSize, SelectedType, SelectedActiveStatus, SearchQuery);
            if (response.Success && response.Data != null)
            {
                Customers = new ObservableCollection<CustomerSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل قائمة العملاء";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task CreateCustomerAsync()
    {
        if (OpenDialogHandler != null)
        {
            await OpenDialogHandler(null);
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task EditCustomerAsync(object? parameter)
    {
        if (parameter is CustomerSummaryDto customer && OpenDialogHandler != null)
        {
            await OpenDialogHandler(customer);
            await LoadCustomersAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleActiveStatusAsync(object? parameter)
    {
        if (parameter is not CustomerSummaryDto customer) return;

        await ExecuteAsync(async () =>
        {
            var res = await _customerApiService.ToggleActiveStatusAsync(customer.Id);
            if (res.Success)
            {
                await LoadCustomersAsync();
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تعديل حالة نشاط العميل";
            }
        });
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(object? parameter)
    {
        if (parameter is not CustomerSummaryDto customer) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف العميل", $"هل أنت متأكد من حذف العميل '{customer.Name}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _customerApiService.DeleteAsync(customer.Id);
            if (res.Success) await LoadCustomersAsync();
            else ErrorMessage = res.Message;
        });
    }
}

public partial class CustomerFormViewModel : BaseViewModel
{
    private readonly ICustomerApiService _customerApiService;

    [ObservableProperty]
    private Guid? _customerId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _code;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private CustomerType _type = CustomerType.Regular;

    [ObservableProperty]
    private decimal _creditLimit;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string _newPhoneNumber = string.Empty;

    [ObservableProperty]
    private string? _newContactName;

    [ObservableProperty]
    private bool _newPhoneIsDefault;

    [ObservableProperty]
    private ObservableCollection<CustomerPhoneDto> _phones = new();

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public CustomerFormViewModel(ICustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
    }

    public async Task InitializeAsync(Guid? id)
    {
        if (id.HasValue)
        {
            IsEditMode = true;
            CustomerId = id.Value;
            await LoadCustomerDetailsAsync(id.Value);
        }
        else
        {
            IsEditMode = false;
            CustomerId = null;
            Name = string.Empty;
            Code = string.Empty;
            Email = string.Empty;
            Address = string.Empty;
            Type = CustomerType.Regular;
            CreditLimit = 0;
            IsActive = true;
            Phones = new ObservableCollection<CustomerPhoneDto>();
        }
    }

    private async Task LoadCustomerDetailsAsync(Guid id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _customerApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                Name = res.Data.Name;
                Code = res.Data.Code;
                Email = res.Data.Email;
                Address = res.Data.Address;
                Type = res.Data.Type;
                CreditLimit = res.Data.CreditLimit;
                IsActive = res.Data.IsActive;
                Phones = new ObservableCollection<CustomerPhoneDto>(res.Data.CustomerPhones);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل العميل";
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

        if (IsEditMode && CustomerId.HasValue)
        {
            await ExecuteAsync(async () =>
            {
                var req = new CreateCustomerPhoneRequest
                {
                    PhoneNumber = NewPhoneNumber,
                    ContactName = NewContactName,
                    IsDefault = NewPhoneIsDefault
                };
                var res = await _customerApiService.AddPhoneAsync(CustomerId.Value, req);
                if (res.Success && res.Data != null)
                {
                    Phones = new ObservableCollection<CustomerPhoneDto>(res.Data.CustomerPhones);
                    NewPhoneNumber = string.Empty;
                    NewContactName = string.Empty;
                    NewPhoneIsDefault = false;
                }
                else ErrorMessage = res.Message;
            });
        }
        else
        {
            Phones.Add(new CustomerPhoneDto
            {
                Id = Guid.NewGuid(),
                PhoneNumber = NewPhoneNumber,
                ContactName = NewContactName,
                IsDefault = NewPhoneIsDefault
            });
            NewPhoneNumber = string.Empty;
            NewContactName = string.Empty;
            NewPhoneIsDefault = false;
        }
    }

    [RelayCommand]
    private async Task DeletePhoneAsync(object? parameter)
    {
        if (parameter is not CustomerPhoneDto phone) return;
        if (IsEditMode && CustomerId.HasValue)
        {
            await ExecuteAsync(async () =>
            {
                var res = await _customerApiService.DeletePhoneAsync(CustomerId.Value, phone.Id);
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
            ErrorMessage = "اسم العميل مطلوب";
            return;
        }

        if (!string.IsNullOrWhiteSpace(NewPhoneNumber))
        {
            if (!Phones.Any(p => p.PhoneNumber == NewPhoneNumber))
            {
                Phones.Add(new CustomerPhoneDto
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber = NewPhoneNumber,
                    ContactName = NewContactName,
                    IsDefault = NewPhoneIsDefault
                });
            }
            NewPhoneNumber = string.Empty;
            NewContactName = string.Empty;
            NewPhoneIsDefault = false;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && CustomerId.HasValue)
            {
                var req = new UpdateCustomerRequest
                {
                    Name = Name,
                    Code = Code,
                    Email = Email,
                    Address = Address,
                    Type = Type,
                    CreditLimit = CreditLimit,
                    IsActive = IsActive,
                    Phones = Phones.Select(p => new CreateCustomerPhoneRequest
                    {
                        PhoneNumber = p.PhoneNumber,
                        ContactName = p.ContactName,
                        IsDefault = p.IsDefault
                    }).ToList()
                };
                var res = await _customerApiService.UpdateAsync(CustomerId.Value, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل حفظ بيانات العميل";
            }
            else
            {
                var phoneRequests = Phones.Select(p => new CreateCustomerPhoneRequest
                {
                    PhoneNumber = p.PhoneNumber,
                    ContactName = p.ContactName,
                    IsDefault = p.IsDefault
                }).ToList();

                var req = new CreateCustomerRequest
                {
                    Name = Name,
                    Code = Code,
                    Email = Email,
                    Address = Address,
                    Type = Type,
                    CreditLimit = CreditLimit,
                    Phones = phoneRequests
                };
                var res = await _customerApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل إضافة العميل";
            }
        });
    }
}
