using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Navigation;
using RetalSystemAPI.Desktop.Models.Auth;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthApiService _authApiService;
    private readonly AuthStateService _authStateService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    public Action? OnLoginSuccess { get; set; }

    public LoginViewModel(
        IAuthApiService authApiService,
        AuthStateService authStateService,
        INavigationService navigationService)
    {
        _authApiService = authApiService;
        _authStateService = authStateService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "يرجى إدخال البريد الإلكتروني وكلمة المرور";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var request = new LoginRequest { UserName = Email, Password = Password };
            var response = await _authApiService.LoginAsync(request);
            if (response.Success && response.Data != null)
            {
                _authStateService.SetToken(
                    response.Data.Token, 
                    response.Data.UserId, 
                    response.Data.TenantId, 
                    RememberMe,
                    response.Data.Roles,
                    response.Data.UserName,
                    response.Data.Permissions);
                OnLoginSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تسجيل الدخول";
            }
        });
    }
}
