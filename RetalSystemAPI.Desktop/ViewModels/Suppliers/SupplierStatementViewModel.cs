using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Suppliers;

public partial class SupplierStatementViewModel : BaseViewModel
{
    private readonly ISupplierApiService _supplierApiService;

    [ObservableProperty]
    private Guid _supplierId;

    [ObservableProperty]
    private string _supplierName = string.Empty;

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
    private ObservableCollection<SupplierTransactionDto> _transactions = new();

    public SupplierStatementViewModel(ISupplierApiService supplierApiService)
    {
        _supplierApiService = supplierApiService;
    }

    public async Task InitializeAsync(Guid supplierId, string supplierName)
    {
        SupplierId = supplierId;
        SupplierName = supplierName;
        FromDate = DateTime.Today.AddDays(-30);
        ToDate = DateTime.Today;

        await LoadStatementAsync();
    }

    [RelayCommand]
    public async Task LoadStatementAsync()
    {
        await ExecuteAsync(async () =>
        {
            var res = await _supplierApiService.GetStatementAsync(SupplierId, FromDate, ToDate, 1, 100);
            if (res.Success && res.Data != null)
            {
                SupplierName = res.Data.SupplierName;
                OpeningBalance = res.Data.OpeningBalance;
                BalanceBeforePeriod = res.Data.BalanceBeforePeriod;
                TotalDebit = res.Data.TotalDebit;
                TotalCredit = res.Data.TotalCredit;
                CurrentBalance = res.Data.CurrentBalance;
                Transactions = new ObservableCollection<SupplierTransactionDto>(res.Data.Transactions);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل كشف حساب المورد";
            }
        });
    }
}
