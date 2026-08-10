using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Auth;

namespace RetalSystemAPI.MAUI.Views.Auth;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
