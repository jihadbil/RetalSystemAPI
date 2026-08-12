using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Purchase;

namespace RetalSystemAPI.Desktop.Views.Purchase;

public partial class PurchaseOrderFormWindow : Window
{
    public PurchaseOrderFormWindow(PurchaseOrderFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
