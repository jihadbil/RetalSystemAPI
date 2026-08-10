using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.Core.Auth;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Auth;

namespace RetalSystemAPI.MAUI.ViewModels.Auth;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthApiService _authApiService;
    private readonly IAuthStateService _authStateService;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(IAuthApiService authApiService, IAuthStateService authStateService)
    {
        _authApiService = authApiService;
        _authStateService = authStateService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "يرجى أدخال اسم المستخدم وكلمة المرور";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        var dto = new LoginDto
        {
            UserName = UserName,
            Password = Password
        };

        var result = await _authApiService.LoginAsync(dto);
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            await _authStateService.SetTokenAsync(result.Data.Token);
            await Shell.Current.GoToAsync("//DashboardPage");
        }
        else
        {
            ErrorMessage = result.Message ?? "بيانات الدخول غير صحيحة";
        }
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//RegisterPage");
    }
}
