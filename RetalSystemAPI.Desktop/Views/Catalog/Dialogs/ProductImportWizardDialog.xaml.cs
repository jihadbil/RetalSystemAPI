using System.Windows;
using System.Windows.Input;
using RetalSystemAPI.Desktop.ViewModels.Catalog;

namespace RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

public partial class ProductImportWizardDialog : Window
{
    public ProductImportWizardDialog(ProductImportWizardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestCloseHandler = () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
