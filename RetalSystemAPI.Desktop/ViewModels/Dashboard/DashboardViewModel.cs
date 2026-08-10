using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RetalSystemAPI.Desktop.ViewModels.Dashboard;

public partial class DashboardViewModel : Base.BaseViewModel
{
    [ObservableProperty]
    private int _tenantsCount = 0;

    [ObservableProperty]
    private int _branchesCount = 0;

    [ObservableProperty]
    private int _productsCount = 0;

    [ObservableProperty]
    private int _categoriesCount = 0;

    public DashboardViewModel()
    {
        _ = LoadStatsAsync();
    }

    public async Task LoadStatsAsync()
    {
        await ExecuteAsync(async () =>
        {
            // قيم افتراضية تجريبية للـ Dashboard
            await Task.Delay(300);
            TenantsCount = 12;
            BranchesCount = 34;
            ProductsCount = 150;
            CategoriesCount = 18;
        });
    }
}
