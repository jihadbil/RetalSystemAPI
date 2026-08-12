using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Purchase;
using RetalSystemAPI.Desktop.ViewModels.Purchase;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseOrdersView : UserControl
{
    public PurchaseOrdersView(PurchaseOrdersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenDialogHandler = OpenDialogAsync;
        viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenDialogAsync(PurchaseOrderSummaryDto? order)
    {
        var app = (App)Application.Current;
        var formVM = app.Services.GetRequiredService<PurchaseOrderFormViewModel>();
        await formVM.InitializeAsync(order?.Id);

        var window = new PurchaseOrderFormWindow(formVM)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        formVM.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}
