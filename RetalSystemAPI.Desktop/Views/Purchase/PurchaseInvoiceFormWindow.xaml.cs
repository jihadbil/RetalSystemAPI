using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Purchase;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseInvoiceFormWindow : Window
{
    public PurchaseInvoiceFormWindow(PurchaseInvoiceFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
