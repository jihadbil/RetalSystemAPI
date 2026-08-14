using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class UserFormWindow : Window
{
    public UserFormWindow(UserFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
