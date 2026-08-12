using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Warehouses;

namespace RetalSystemAPI.Desktop.Views.Warehouses;

public partial class WarehouseFormWindow : Window
{
    public WarehouseFormWindow(WarehouseFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
