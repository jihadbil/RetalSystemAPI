using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Stock;

namespace RetalSystemAPI.Desktop.Views.Stock;

public partial class StockView : UserControl
{
    public StockView(StockViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.OpenSetStockDialogHandler = OpenSetStockDialogAsync;

        // شاشة المخزون مخزنة مؤقتاً في NavigationService، لذا يعمل Loaded مرة واحدة فقط.
        // عند إعادة إظهار الشاشة نعيد تحميل قائمة المخازن/الصالات لتظهر الإضافات الجديدة.
        IsVisibleChanged += OnIsVisibleChanged;
    }

    private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // إعادة التحميل فقط عند الانتقال من مخفي إلى ظاهر، وتجنب الازدواج قبل التحميل الأول
        if (e.NewValue is bool visible && visible && IsLoaded)
        {
            if (DataContext is StockViewModel vm)
            {
                _ = vm.RefreshOnNavigatedAsync();
            }
        }
    }

    private Task OpenSetStockDialogAsync(WarehouseSummaryDto? warehouse, object? stockItem)
    {
        if (warehouse == null) return Task.CompletedTask;

        var app = (App)Application.Current;
        var formVM = app.Services.GetRequiredService<SetStockFormViewModel>();
        formVM.Initialize(warehouse, stockItem);

        var window = new SetStockFormWindow(formVM)
        {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        formVM.CloseWindowHandler = () => window.DialogResult = true;
        window.ShowDialog();
        return Task.CompletedTask;
    }
}
