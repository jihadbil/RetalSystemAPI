using System.Windows;
using System.Windows.Input;
using RetalSystemAPI.Desktop.ViewModels.Catalog;

namespace RetalSystemAPI.Desktop.Views.Catalog.Dialogs;

public partial class CategoryFormDialog : Window
{
    public CategoryFormDialog(CategoryFormViewModel viewModel)
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
