using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.ViewModels.Purchase;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseReturnsView : UserControl
{
    private readonly PurchaseReturnsViewModel _viewModel;

    public PurchaseReturnsView(PurchaseReturnsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenReturnDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenReturnDialogAsync(PurchaseReturnSummaryDto? returnDto)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<PurchaseReturnFormViewModel>();

        if (returnDto != null)
        {
            await formVm.InitializeForViewAsync(returnDto.Id);
        }
        else
        {
            await formVm.InitializeForCreateAsync();
        }

        var window = new PurchaseReturnFormWindow
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
            "تأكيد الحذف", "تراجع", RetalSystemAPI.Desktop.Controls.ConfirmDialogType.Danger));
    }
}
