using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Tenant;

namespace RetalSystemAPI.MAUI.ViewModels.Tenants;

public partial class TenantsViewModel : ObservableObject
{
    private readonly ITenantApiService _tenantApiService;

    [ObservableProperty]
    private ObservableCollection<TenantResponseDto> _tenants = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public TenantsViewModel(ITenantApiService tenantApiService)
    {
        _tenantApiService = tenantApiService;
    }

    [RelayCommand]
    private async Task LoadTenantsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await _tenantApiService.GetAllAsync();
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Tenants = new ObservableCollection<TenantResponseDto>(result.Data);
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل قائمة المستأجرين";
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(TenantResponseDto tenant)
    {
        if (tenant == null) return;

        var result = await _tenantApiService.ToggleActiveStatusAsync(tenant.Id);
        if (result.Success)
        {
            await LoadTenantsAsync();
        }
    }
}
