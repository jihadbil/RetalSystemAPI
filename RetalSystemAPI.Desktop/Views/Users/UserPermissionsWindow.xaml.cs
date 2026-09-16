using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class UserPermissionsWindow : Window
{
    public UserPermissionsWindow(UserPermissionsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose = Close;
    }
}
