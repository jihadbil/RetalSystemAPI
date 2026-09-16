using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Purchase;
using RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseInvoiceFormWindow : Window
{
    public PurchaseInvoiceFormWindow(PurchaseInvoiceFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenProductDialogHandler = async (prefillBarcode) =>
        {
            var app = (App)Application.Current;
            var dialogVM = app.Services.GetRequiredService<ProductFormViewModel>();
            var initialProduct = !string.IsNullOrWhiteSpace(prefillBarcode)
                ? new ProductDto { DefaultBarCode = prefillBarcode.Trim(), Code = prefillBarcode.Trim() }
                : null;
            await dialogVM.InitializeAsync(initialProduct);

            var dialog = new ProductFormDialog(dialogVM)
            {
                Owner = this
            };

            dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
            var result = dialog.ShowDialog();
            return result == true ? dialogVM.CreatedOrUpdatedProduct : null;
        };

        viewModel.OpenUnitDialogHandler = () =>
        {
            var app = (App)Application.Current;
            var dialogVM = app.Services.GetRequiredService<UnitFormViewModel>();
            dialogVM.Initialize(null);

            var dialog = new UnitFormDialog(dialogVM)
            {
                Owner = this
            };

            dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
            var result = dialog.ShowDialog();
            return Task.FromResult(result == true ? dialogVM.CreatedUnit : null);
        };

        viewModel.OpenCategoryDialogHandler = () =>
        {
            var app = (App)Application.Current;
            var dialogVM = app.Services.GetRequiredService<CategoryFormViewModel>();
            dialogVM.Initialize(null);

            var dialog = new CategoryFormDialog(dialogVM)
            {
                Owner = this
            };

            dialogVM.CloseWindowHandler = () => dialog.DialogResult = true;
            var result = dialog.ShowDialog();
            return Task.FromResult(result == true ? dialogVM.CreatedCategory : null);
        };
    }
}
