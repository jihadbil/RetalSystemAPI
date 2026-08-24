using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.ViewModels.Sales;

namespace RetalSystemAPI.Desktop.Views.Sales;

public partial class SalesInvoicesView : UserControl
{
    private readonly SalesInvoicesViewModel _viewModel;

    public SalesInvoicesView(SalesInvoicesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenInvoiceDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenInvoiceDialogAsync(SalesInvoiceSummaryDto? invoice)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<SalesInvoiceFormViewModel>();
        await formVm.InitializeAsync(invoice?.Id);

        var window = new SalesInvoiceFormWindow
        {
            DataContext = formVm,
            Owner = Application.Current.MainWindow
        };

        formVm.CloseWindowHandler = () => window.Close();
        window.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}
