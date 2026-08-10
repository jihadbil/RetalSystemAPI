using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Tenant;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Tenants;

public partial class TenantsViewModel : BaseViewModel
{
    private readonly ITenantApiService _tenantApiService;

    [ObservableProperty]
    private TenantDto? _currentTenant;

    [ObservableProperty]
    private string _tenantName = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public TenantsViewModel(ITenantApiService tenantApiService)
    {
        _tenantApiService = tenantApiService;
        _ = LoadProfileAsync();
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        await ExecuteAsync(async () =>
        {
            var response = await _tenantApiService.GetMyProfileAsync();
            if (response.Success && response.Data != null)
            {
                CurrentTenant = response.Data;
                TenantName = response.Data.Name;
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل بيانات الملف الشخصي للمستأجر";
            }
        });
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TenantName))
        {
            ErrorMessage = "اسم المستأجر مطلوب";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var request = new UpdateTenantRequest { Name = TenantName };
            var response = await _tenantApiService.UpdateMyProfileAsync(request);
            if (response.Success && response.Data != null)
            {
                CurrentTenant = response.Data;
                TenantName = response.Data.Name;
                SuccessMessage = "تم حفظ تعديلات الملف الشخصي بنجاح";
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحديث البيانات";
            }
        });
    }
}
