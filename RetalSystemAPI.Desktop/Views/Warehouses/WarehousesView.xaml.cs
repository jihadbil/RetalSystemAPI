using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Warehouses;

namespace RetalSystemAPI.Desktop.Views.Warehouses;

public partial class WarehousesView : UserControl
{
    public WarehousesView(WarehousesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenDialogHandler = OpenDialogAsync;
        viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenDialogAsync(WarehouseSummaryDto? warehouse)
    {
        var app = (App)Application.Current;
        var formVM = app.Services.GetRequiredService<WarehouseFormViewModel>();
        await formVM.InitializeAsync(warehouse);

        var window = new WarehouseFormWindow(formVM)
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
