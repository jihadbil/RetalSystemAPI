using System.Windows.Controls;
using RetalSystemAPI.Desktop.ViewModels.Tenants;

namespace RetalSystemAPI.Desktop.Views.Tenants;

public partial class TenantsView : UserControl
{
    public TenantsView(TenantsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
