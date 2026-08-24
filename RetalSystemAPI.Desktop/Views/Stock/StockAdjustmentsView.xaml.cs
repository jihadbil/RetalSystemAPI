using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Stock;

namespace RetalSystemAPI.Desktop.Views.Stock;

public partial class StockAdjustmentsView : UserControl
{
    private readonly StockAdjustmentsViewModel _viewModel;

    public StockAdjustmentsView(StockAdjustmentsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenAdjustmentDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenAdjustmentDialogAsync(StockAdjustmentSummaryDto? adjustment)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<StockAdjustmentFormViewModel>();
        await formVm.InitializeAsync(adjustment?.Id);

        var window = new StockAdjustmentFormWindow
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
