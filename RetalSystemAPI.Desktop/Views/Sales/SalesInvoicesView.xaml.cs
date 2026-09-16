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

        formVm.CloseWindowHandler = () => window.DialogResult = true;
        formVm.OpenReturnDialogHandler = (invoiceId, item) => OpenReturnFromInvoiceItemAsync(window, invoiceId, item);
        window.ShowDialog();
    }

    /// <summary>فتح نموذج مرتجع مبيعات مربوط بالفاتورة وبدءه ببند البند المحدد من الفاتورة</summary>
    private async Task OpenReturnFromInvoiceItemAsync(Window owner, Guid invoiceId, CreateSalesInvoiceItemRequest item)
    {
        var app = (App)Application.Current;
        var returnVm = app.Services.GetRequiredService<SalesReturnFormViewModel>();

        var prefilled = new CreateSalesReturnItemRequest
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = 1,
            UnitPrice = item.UnitPrice
        };
        await returnVm.InitializeForInvoiceAsync(invoiceId, prefilled);

        var returnWindow = new SalesReturnFormWindow
        {
            DataContext = returnVm,
            Owner = owner
        };

        returnVm.CloseWindowHandler = () => returnWindow.DialogResult = true;
        returnWindow.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(RetalSystemAPI.Desktop.Controls.ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow, title, message,
            "تأكيد", "تراجع", RetalSystemAPI.Desktop.Controls.ConfirmDialogType.Danger));
    }
}
