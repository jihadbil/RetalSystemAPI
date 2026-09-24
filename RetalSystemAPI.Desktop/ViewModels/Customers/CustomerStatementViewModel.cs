using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Customers;

public partial class CustomerStatementViewModel : BaseViewModel
{
    private readonly ICustomerApiService _customerApiService;

    [ObservableProperty]
    private Guid _customerId;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string? _customerCode;

    [ObservableProperty]
    private decimal _creditLimit;

    [ObservableProperty]
    private decimal _openingBalance;

    [ObservableProperty]
    private decimal _balanceBeforePeriod;

    [ObservableProperty]
    private decimal _totalDebit;

    [ObservableProperty]
    private decimal _totalCredit;

    [ObservableProperty]
    private decimal _currentBalance;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private ObservableCollection<CustomerTransactionDto> _transactions = new();

    public CustomerStatementViewModel(ICustomerApiService customerApiService)
    {
        _customerApiService = customerApiService;
    }

    public async Task InitializeAsync(Guid customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        FromDate = DateTime.Today.AddDays(-30);
        ToDate = DateTime.Today;

        await LoadStatementAsync();
    }

    [RelayCommand]
    public async Task LoadStatementAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _customerApiService.GetStatementAsync(CustomerId, FromDate, ToDate, 1, 100);
            if (res.Success && res.Data != null)
            {
                CustomerName = res.Data.CustomerName;
                CustomerCode = res.Data.CustomerCode;
                CreditLimit = res.Data.CreditLimit;
                OpeningBalance = res.Data.OpeningBalance;
                BalanceBeforePeriod = res.Data.BalanceBeforePeriod;
                TotalDebit = res.Data.TotalDebit;
                TotalCredit = res.Data.TotalCredit;
                CurrentBalance = res.Data.CurrentBalance;
                Transactions = new ObservableCollection<CustomerTransactionDto>(res.Data.Transactions);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل كشف حساب العميل";
            }
        });
    }
}
