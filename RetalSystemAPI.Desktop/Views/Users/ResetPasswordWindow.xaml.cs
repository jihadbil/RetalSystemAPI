using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class ResetPasswordWindow : Window
{
    public ResetPasswordWindow(ResetPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closed += (_, _) => { viewModel.NewPassword = string.Empty; viewModel.ConfirmPassword = string.Empty; };
    }

    private void NewPassword_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is ResetPasswordViewModel model && sender is System.Windows.Controls.PasswordBox box)
            model.NewPassword = box.Password;
    }

    private void ConfirmPassword_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is ResetPasswordViewModel model && sender is System.Windows.Controls.PasswordBox box)
            model.ConfirmPassword = box.Password;
    }
}
