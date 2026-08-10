using System.Windows.Controls;
using RetalSystemAPI.Desktop.ViewModels.Auth;

namespace RetalSystemAPI.Desktop.Views.Auth;

public partial class LoginView : UserControl
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TxtPassword_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = TxtPassword.Password;
        }
    }
}
