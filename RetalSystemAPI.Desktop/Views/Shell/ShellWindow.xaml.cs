using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Shell;

namespace RetalSystemAPI.Desktop.Views.Shell;

public partial class ShellWindow : Window
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
