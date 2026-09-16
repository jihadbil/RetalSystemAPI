using System.Windows;
using RetalSystemAPI.Desktop.ViewModels.Shell;

namespace RetalSystemAPI.Desktop.Views.Shell;

public partial class ShellWindow : Window
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => SetSidebarMode(ActualWidth < 1200);
        SizeChanged += (_, _) =>
        {
            if (ActualWidth < 1100 && DataContext is ShellViewModel vm && !vm.IsSidebarCollapsed)
            {
                SetSidebarMode(true);
            }
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.F11)
            {
                if (DataContext is ShellViewModel vm)
                {
                    SetSidebarMode(!vm.IsSidebarCollapsed);
                }
                e.Handled = true;
            }
        };
        Closing += (_, _) => viewModel.SaveWorkspace();
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ShellViewModel.ActiveSection))
            {
                // فتح تصنيف الشاشة المختارة تلقائياً إن كان مغلقاً فقط، دون إغلاق بقية التصنيفات المفتوحة
                foreach (var group in Descendants<System.Windows.Controls.Expander>(Sidebar))
                {
                    if (group.IsExpanded) continue;

                    bool containsActiveSection = Descendants<System.Windows.Controls.Button>(group).Any(button =>
                        System.Windows.Data.BindingOperations.GetBinding(button, System.Windows.Controls.Control.BackgroundProperty)?.ConverterParameter?.ToString() == viewModel.ActiveSection);
                    if (containsActiveSection)
                    {
                        group.IsExpanded = true;
                    }
                }
            }
            else if (args.PropertyName == nameof(ShellViewModel.IsSidebarCollapsed))
            {
                SetSidebarMode(viewModel.IsSidebarCollapsed);
            }
        };
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ShellViewModel vm)
        {
            SetSidebarMode(!vm.IsSidebarCollapsed);
        }
    }

    private void SetSidebarMode(bool isCollapsed)
    {
        if (DataContext is ShellViewModel vm && vm.IsSidebarCollapsed != isCollapsed)
        {
            vm.IsSidebarCollapsed = isCollapsed;
        }
        SidebarColumn.Width = new GridLength(isCollapsed ? 64 : 232);
        Sidebar.Visibility = Visibility.Visible;
    }
}
