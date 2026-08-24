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

        formVm.CloseAction = () => window.Close();
        window.ShowDialog();

        return formVm.SaveSuccessful;
    }

    private Task<bool> ConfirmActionAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}
