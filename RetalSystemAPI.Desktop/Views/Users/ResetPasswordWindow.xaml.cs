using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class ResetPasswordWindow : Window
{
    public ResetPasswordWindow(ResetPasswordViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
