using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.ViewModels.Sales;

namespace RetalSystemAPI.Desktop.Views.Sales;

public partial class SalesReturnsView : UserControl
{
    private readonly SalesReturnsViewModel _viewModel;

    public SalesReturnsView(SalesReturnsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenReturnDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenReturnDialogAsync(SalesReturnSummaryDto? returnDto)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<SalesReturnFormViewModel>();
        await formVm.InitializeAsync(returnDto?.Id);

        var window = new SalesReturnFormWindow
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
