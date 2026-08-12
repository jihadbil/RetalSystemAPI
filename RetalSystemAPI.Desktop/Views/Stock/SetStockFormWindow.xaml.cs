using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Stock;

namespace RetalSystemAPI.Desktop.Views.Stock;

public partial class SetStockFormWindow : Window
{
    public SetStockFormWindow(SetStockFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
