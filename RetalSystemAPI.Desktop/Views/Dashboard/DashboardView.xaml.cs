using System.Windows.Controls;
using RetalSystemAPI.Desktop.ViewModels.Dashboard;

namespace RetalSystemAPI.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
