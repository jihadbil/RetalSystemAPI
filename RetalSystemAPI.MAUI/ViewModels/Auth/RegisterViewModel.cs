using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.Core.Auth;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Auth;

namespace RetalSystemAPI.MAUI.ViewModels.Auth;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthApiService _authApiService;
    private readonly IAuthStateService _authStateService;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private Guid _tenantId = Guid.Empty;

    [ObservableProperty]
    private Guid _branchId = Guid.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public RegisterViewModel(IAuthApiService authApiService, IAuthStateService authStateService)
    {
        _authApiService = authApiService;
        _authStateService = authStateService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "يرجى تعبئة جميع الحقول المطلوبة";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        var dto = new RegisterDto
        {
            UserName = UserName,
            Email = Email,
            Password = Password,
            TenantId = TenantId,
            BranchId = BranchId
        };

        var result = await _authApiService.RegisterAsync(dto);
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            await _authStateService.SetTokenAsync(result.Data.Token);
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل إنشاء الحساب";
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
