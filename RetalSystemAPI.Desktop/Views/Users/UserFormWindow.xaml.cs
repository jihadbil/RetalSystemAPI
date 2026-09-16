using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.ViewModels.Users;

namespace RetalSystemAPI.Desktop.Views.Users;

public partial class UserFormWindow : Window
{
    public UserFormWindow(UserFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closed += (_, _) => viewModel.Password = string.Empty;

        viewModel.OpenResetPasswordHandler = (id, name) =>
        {
            var app = (App)Application.Current;
            var resetVM = app.Services.GetRequiredService<ResetPasswordViewModel>();
            resetVM.Initialize(id, name);
            var window = new ResetPasswordWindow(resetVM)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            resetVM.CloseWindowHandler = () => window.DialogResult = true;
            window.ShowDialog();
            return Task.CompletedTask;
        };
    }

    private void Password_Changed(object sender, RoutedEventArgs e)
    {
        if (DataContext is UserFormViewModel model && sender is System.Windows.Controls.PasswordBox box)
            model.Password = box.Password;
    }
}
