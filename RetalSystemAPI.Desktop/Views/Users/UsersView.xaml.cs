using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Users;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class UsersView : UserControl
{
    public UsersView(UsersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenFormDialogHandler = OpenFormDialogAsync;
        viewModel.OpenResetPasswordDialogHandler = OpenResetPasswordDialogAsync;
        viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenFormDialogAsync(UserSummaryDto? user)
    {
        var app = (App)Application.Current;
        var formVM = app.Services.GetRequiredService<UserFormViewModel>();
        await formVM.InitializeAsync(user?.Id);

        var window = new UserFormWindow(formVM)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        formVM.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task OpenResetPasswordDialogAsync(UserSummaryDto user)
    {
        var app = (App)Application.Current;
        var resetVM = app.Services.GetRequiredService<ResetPasswordViewModel>();
        resetVM.Initialize(user.Id, user.UserName);

        var window = new ResetPasswordWindow(resetVM)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        resetVM.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
        return Task.CompletedTask;
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}
