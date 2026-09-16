using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Stock;

namespace RetalSystemAPI.Desktop.Views.Stock;

public partial class StockTransfersView : UserControl
{
    private readonly StockTransfersViewModel _viewModel;

    public StockTransfersView(StockTransfersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenTransferDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenTransferDialogAsync(StockTransferSummaryDto? transfer)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<StockTransferFormViewModel>();
        await formVm.InitializeAsync(transfer?.Id);

        var window = new StockTransferFormWindow
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
