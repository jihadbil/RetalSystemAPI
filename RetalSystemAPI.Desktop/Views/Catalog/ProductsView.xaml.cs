using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

namespace RetalSystemAPI.Desktop.Views.Catalog;

public partial class ProductsView : UserControl
{
    public ProductsView(ProductsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OpenDialogHandler = OpenProductDialog;
        viewModel.OpenImportWizardHandler = OpenImportWizardDialog;
        viewModel.ConfirmDeleteHandler = (title, message) =>
        {
            var owner = Window.GetWindow(this);
            bool confirmed = ModernConfirmDialog.ShowConfirm(owner, title, message, "تأكيد الحذف", "إلغاء", ConfirmDialogType.Danger);
            return Task.FromResult(confirmed);
        };
    }

    private Task OpenImportWizardDialog()
    {
        var app = (App)Application.Current;
        var wizardVM = app.Services.GetRequiredService<ProductImportWizardViewModel>();

        var dialog = new ProductImportWizardDialog(wizardVM)
        {
            Owner = Window.GetWindow(this)
        };

        dialog.ShowDialog();
        return Task.CompletedTask;
    }

    private async Task OpenProductDialog(ProductDto? product)
    {
        var app = (App)Application.Current;
        var dialogVM = app.Services.GetRequiredService<ProductFormViewModel>();
        await dialogVM.InitializeAsync(product);

        var dialog = new ProductFormDialog(dialogVM)
        {
            Owner = Window.GetWindow(this)
        };

        dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
        dialog.ShowDialog();
    }
}
