using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.ViewModels.Customers;

namespace RetalSystemAPI.Desktop.Views.Customers;

public partial class CustomersView : UserControl
{
    private readonly CustomersViewModel _viewModel;

    public CustomersView(CustomersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenCustomerDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenCustomerDialogAsync(CustomerSummaryDto? customer)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<CustomerFormViewModel>();
        await formVm.InitializeAsync(customer?.Id);

        var window = new CustomerFormWindow
        {
            DataContext = formVm,
            Owner = Application.Current.MainWindow
        };

        formVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(RetalSystemAPI.Desktop.Controls.ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow, title, message,
            "تأكيد", "تراجع", RetalSystemAPI.Desktop.Controls.ConfirmDialogType.Danger));
    }
}
