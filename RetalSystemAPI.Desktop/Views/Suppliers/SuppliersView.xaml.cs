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
}
