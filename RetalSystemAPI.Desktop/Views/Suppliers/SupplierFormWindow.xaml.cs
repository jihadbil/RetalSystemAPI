using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Suppliers;

namespace RetalSystemAPI.Desktop.Views.Suppliers;

public partial class SupplierFormWindow : Window
{
    public SupplierFormWindow(SupplierFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
