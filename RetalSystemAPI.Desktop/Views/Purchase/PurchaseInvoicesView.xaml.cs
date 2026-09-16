using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.ViewModels.Purchase;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseInvoicesView : UserControl
{
    private readonly PurchaseInvoicesViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public PurchaseInvoicesView(PurchaseInvoicesViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = _viewModel;

        _viewModel.OpenFormHandler = OpenFormAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmActionAsync;
    }

    private async Task<bool> OpenFormAsync(PurchaseInvoiceDto? invoice)
    {
        var formVm = _serviceProvider.GetRequiredService<PurchaseInvoiceFormViewModel>();
        await formVm.InitializeAsync(invoice);

        var window = new PurchaseInvoiceFormWindow(formVm)
        {
            Owner = Window.GetWindow(this)
        };

        formVm.CloseAction = () =>
        {
            if (formVm.SaveSuccessful) window.DialogResult = true;
            else window.Close();
        };
        formVm.OpenReturnDialogHandler = (invoiceId, item) => OpenReturnFromInvoiceItemAsync(window, invoiceId, item);
        window.ShowDialog();

        return formVm.SaveSuccessful;
    }

    /// <summary>فتح نموذج مرتجع المشتريات مربوط بفاتورة مغلقة وبدءه ببند الفاتورة المحدد</summary>
    private async Task OpenReturnFromInvoiceItemAsync(Window owner, Guid invoiceId, CreatePurchaseInvoiceItemRequest item)
    {
        var returnVm = _serviceProvider.GetRequiredService<PurchaseReturnFormViewModel>();

        var prefilled = new CreatePurchaseReturnItemRequest
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            ProductBarCodeId = item.ProductBarCodeId,
            BarcodeTitle = !string.IsNullOrWhiteSpace(item.BarCode) ? item.BarCode : "افتراضي",
            Quantity = 1,
            UnitPrice = item.UnitPrice
        };
        await returnVm.InitializeForInvoiceAsync(invoiceId, prefilled);

        var returnWindow = new PurchaseReturnFormWindow
        {
            DataContext = returnVm,
            Owner = owner
        };

        returnVm.CloseWindowHandler = () => returnWindow.DialogResult = true;
        returnWindow.ShowDialog();
    }

    private Task<bool> ConfirmActionAsync(string title, string message)
    {
        return Task.FromResult(RetalSystemAPI.Desktop.Controls.ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow, title, message,
            "تأكيد", "تراجع", RetalSystemAPI.Desktop.Controls.ConfirmDialogType.Danger));
    }
}
