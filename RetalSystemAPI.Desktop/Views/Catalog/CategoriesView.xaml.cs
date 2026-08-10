using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

namespace RetalSystemAPI.Desktop.Views.Catalog;

public partial class CategoriesView : UserControl
{
    public CategoriesView(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OpenDialogHandler = OpenCategoryDialog;
        viewModel.ConfirmDeleteHandler = (title, message) =>
        {
            var owner = Window.GetWindow(this);
            bool confirmed = ModernConfirmDialog.ShowConfirm(owner, title, message, "تأكيد الحذف", "إلغاء", ConfirmDialogType.Danger);
            return Task.FromResult(confirmed);
        };
    }

    private Task OpenCategoryDialog(CategoryDto? category)
    {
        var app = (App)Application.Current;
        var dialogVM = app.Services.GetRequiredService<CategoryFormViewModel>();
        dialogVM.Initialize(category);

        var dialog = new CategoryFormDialog(dialogVM)
        {
            Owner = Window.GetWindow(this)
        };

        dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
        dialog.ShowDialog();

        return Task.CompletedTask;
    }
}
