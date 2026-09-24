using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Accounting;
using RetalSystemAPI.Desktop.ViewModels.Accounting;

namespace RetalSystemAPI.Desktop.Views.Accounting;

public partial class ExpenseCategoriesView : UserControl
{
    private readonly ExpenseCategoriesViewModel _viewModel;

    public ExpenseCategoriesView(ExpenseCategoriesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.OpenDialogHandler = OpenCategoryDialogAsync;
        _viewModel.ConfirmDeleteHandler = ConfirmDeleteAsync;
    }

    private Task OpenCategoryDialogAsync(ExpenseCategoryDto? category)
    {
        var app = (App)Application.Current;
        var formVm = app.Services.GetRequiredService<ExpenseCategoryFormViewModel>();
        formVm.Initialize(category);

        var window = new ExpenseCategoryFormWindow
        {
            DataContext = formVm,
            Owner = Application.Current.MainWindow
        };

        formVm.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
        return Task.CompletedTask;
    }

    private Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        return Task.FromResult(ModernConfirmDialog.ShowConfirm(
            Window.GetWindow(this) ?? Application.Current.MainWindow,
            title, message, "تأكيد الحذف", "تراجع", ConfirmDialogType.Danger));
    }
}
