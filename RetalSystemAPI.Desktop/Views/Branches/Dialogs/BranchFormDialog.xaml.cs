using System.Windows;
using System.Windows.Input;
using RetalSystemAPI.Desktop.ViewModels.Branches;

namespace RetalSystemAPI.Desktop.Views.Branches.Dialogs;

public partial class BranchFormDialog : Window
{
    public BranchFormDialog(BranchFormViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
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
