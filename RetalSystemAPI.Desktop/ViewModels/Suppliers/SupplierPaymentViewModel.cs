using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Accounting;
using RetalSystemAPI.Desktop.Services.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Suppliers;

public partial class SupplierPaymentViewModel : BaseViewModel
{
    private readonly ISupplierApiService _supplierApiService;
    private readonly ISafeApiService _safeApiService;
    private readonly IBranchApiService _branchApiService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private Guid _supplierId;

    [ObservableProperty]
    private string _supplierName = string.Empty;

    [ObservableProperty]
    private decimal _supplierCurrentBalance;

    [ObservableProperty, NotifyDataErrorInfo]
    [Range(0.01, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
    private decimal _amount;

    [ObservableProperty]
    private PaymentMethod _paymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<SafeSummaryDto> _safes = new();

    [ObservableProperty]
    private SafeSummaryDto? _selectedSafe;

    [ObservableProperty]
    private DateTime _paymentDate = DateTime.Today;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _notes;

    public Action? CloseWindowHandler { get; set; }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        _ = LoadSafesForBranchAsync(value?.Id);
    }

    public SupplierPaymentViewModel(
        ISupplierApiService supplierApiService,
        ISafeApiService safeApiService,
        IBranchApiService branchApiService,
        IToastService toastService)
    {
        _supplierApiService = supplierApiService;
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

    public async Task InitializeAsync(Guid supplierId, string supplierName, decimal currentBalance)
    {
        SupplierId = supplierId;
        SupplierName = supplierName;
        SupplierCurrentBalance = currentBalance;
        Amount = 0;
        PaymentMethod = PaymentMethod.Cash;
        PaymentDate = DateTime.Today;
        Description = $"سند صرف / سداد للمورد {supplierName}";
        Notes = null;

        var branchesRes = await _branchApiService.GetAllAsync();
        if (branchesRes.Success && branchesRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchesRes.Data);
            SelectedBranch = Branches.FirstOrDefault();
            if (SelectedBranch != null)
            {
                await LoadSafesForBranchAsync(SelectedBranch.Id);
            }
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        if (HasErrors) return;

        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب تحديد الفرع";
            return;
        }

        if (SelectedSafe != null && Amount > SelectedSafe.CurrentBalance)
        {
            ErrorMessage = $"رصيد الخزينة المحدد ({SelectedSafe.CurrentBalance:N2}) غير كافٍ لصرف مبلغ ({Amount:N2})";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreateSupplierPaymentRequest
            {
                SupplierId = SupplierId,
                BranchId = SelectedBranch.Id,
                SafeId = SelectedSafe?.Id,
                Amount = Amount,
                PaymentMethod = PaymentMethod,
                PaymentDate = PaymentDate,
                Description = Description?.Trim(),
                Notes = Notes?.Trim()
            };

            var res = await _supplierApiService.AddPaymentAsync(SupplierId, req);
            if (res.Success)
            {
                _toastService.ShowSuccess("تم تسجيل سند الصرف وخصم المبلغ من الخزينة بنجاح");
                CloseWindowHandler?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message;
            }
        });
    }
}
