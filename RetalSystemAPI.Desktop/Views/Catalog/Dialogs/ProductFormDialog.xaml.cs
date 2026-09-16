using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.ViewModels.Catalog;

namespace RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

public partial class ProductFormDialog : Window
{
    public ProductFormDialog(ProductFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

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
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
