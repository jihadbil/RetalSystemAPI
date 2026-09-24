using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Accounting;

namespace RetalSystemAPI.Desktop.Views.Accounting;

public partial class ExpensesView : UserControl
{
    private readonly ExpensesViewModel _viewModel;

    public ExpensesView(ExpensesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenExpenseDialogHandler = OpenExpenseDialogAsync;
        _viewModel.PromptCancelHandler = PromptCancelReasonAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private async Task OpenExpenseDialogAsync(ExpenseSummaryDto? expense)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<ExpenseFormViewModel>();
        await formVm.InitializeAsync(expense?.Id);

        var window = new ExpenseFormWindow
        {
            DataContext = formVm,
            Owner = Application.Current.MainWindow
        };

        formVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
    }

    private Task<string?> PromptCancelReasonAsync(ExpenseSummaryDto expense)
    {
        var confirmed = ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow,
            "تأكيد إلغاء سند المصروف",
            $"هل أنت متأكد من رغبتك في إلغاء سند المصروف رقم '{expense.ExpenseNumber}' بقيمة ({expense.Amount:N2} د.ل) واسترجاع المبلغ للخزينة؟",
            "نعم، إلغاء السند", "تراجع", ConfirmDialogType.Warning);

        if (!confirmed) return Task.FromResult<string?>(null);
        return Task.FromResult<string?>("تم الإلغاء بطلب من المستخدم من شاشة المصروفات");
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow,
            title, message, "تأكيد الحذف", "تراجع", ConfirmDialogType.Danger));
    }
}
