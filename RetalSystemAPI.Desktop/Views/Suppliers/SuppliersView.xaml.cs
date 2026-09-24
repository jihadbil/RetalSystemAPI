using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Suppliers;
using RetalSystemAPI.Desktop.ViewModels.Suppliers;

namespace RetalSystemAPI.Desktop.Views.Suppliers;

public partial class SuppliersView : UserControl
{
    public SuppliersView(SuppliersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenDialogHandler = OpenDialogAsync;
        viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
        viewModel.OpenStatementHandler = OpenStatementAsync;
        viewModel.OpenPaymentHandler = OpenPaymentAsync;
    }

    private async Task OpenDialogAsync(SupplierSummaryDto? supplier)
    {
        var app = (App)Application.Current;
        var formVM = app.Services.GetRequiredService<SupplierFormViewModel>();
        await formVM.InitializeAsync(supplier?.Id);

        var window = new SupplierFormWindow(formVM)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        formVM.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(RetalSystemAPI.Desktop.Controls.ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow, title, message,
            "تأكيد", "تراجع", RetalSystemAPI.Desktop.Controls.ConfirmDialogType.Danger));
    }

    private async Task OpenStatementAsync(SupplierSummaryDto supplier)
    {
        var app = (App)Application.Current;
        var statementVm = app.Services.GetRequiredService<SupplierStatementViewModel>();
        await statementVm.InitializeAsync(supplier.Id, supplier.Name);

        var window = new SupplierStatementWindow
        {
            DataContext = statementVm,
            Owner = Application.Current.MainWindow
        };

        window.ShowDialog();
    }

    private async Task OpenPaymentAsync(SupplierSummaryDto supplier)
    {
        var app = (App)Application.Current;
        var paymentVm = app.Services.GetRequiredService<SupplierPaymentViewModel>();
        await paymentVm.InitializeAsync(supplier.Id, supplier.Name, supplier.CurrentBalance);

        var window = new SupplierPaymentWindow
        {
            DataContext = paymentVm,
            Owner = Application.Current.MainWindow
        };

        paymentVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }
}
