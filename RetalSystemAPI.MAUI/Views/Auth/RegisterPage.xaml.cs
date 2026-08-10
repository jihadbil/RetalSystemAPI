using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Auth;

namespace RetalSystemAPI.MAUI.Views.Auth;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
