using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Accounting;

namespace RetalSystemAPI.Desktop.Views.Accounting;

public partial class SafesView : UserControl
{
    private readonly SafesViewModel _viewModel;

    public SafesView(SafesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenSafeDialogHandler = OpenSafeDialogAsync;
        _viewModel.OpenTransferDialogHandler = OpenTransferDialogAsync;
        _viewModel.OpenTransactionDialogHandler = OpenTransactionDialogAsync;
        _viewModel.OpenLedgerHandler = OpenLedgerAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenSafeDialogAsync(SafeSummaryDto? safe)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<SafeFormViewModel>();
        await formVm.InitializeAsync(safe?.Id);

        var window = new SafeFormWindow
        {
            DataContext = formVm,
            Owner = Application.Current.MainWindow
        };

        formVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private async Task OpenTransferDialogAsync(SafeSummaryDto? safe)
    {
        var app = (App)Application.Current;
        var transferVm = app.Services.GetRequiredService<SafeTransferViewModel>();
        await transferVm.InitializeAsync(safe?.Id);

        var window = new SafeTransferWindow
        {
            DataContext = transferVm,
            Owner = Application.Current.MainWindow
        };

        transferVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task OpenTransactionDialogAsync(SafeSummaryDto? safe)
    {
        if (safe == null) return Task.CompletedTask;
        var app = (App)Application.Current;
        var txVm = app.Services.GetRequiredService<SafeTransactionFormViewModel>();
        txVm.Initialize(safe);

        var window = new SafeTransactionFormWindow
        {
            DataContext = txVm,
            Owner = Application.Current.MainWindow
        };

        txVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
        return Task.CompletedTask;
    }

    private async Task OpenLedgerAsync(SafeSummaryDto safe)
    {
        var app = (App)Application.Current;
        var ledgerVm = app.Services.GetRequiredService<SafeLedgerViewModel>();
        await ledgerVm.InitializeAsync(safe.Id, safe.Name, safe.CurrentBalance);

        var window = new SafeLedgerWindow
        {
            DataContext = ledgerVm,
            Owner = Application.Current.MainWindow
        };

        window.ShowDialog();
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow,
            title, message, "تأكيد الحذف", "تراجع", ConfirmDialogType.Danger));
    }
}
