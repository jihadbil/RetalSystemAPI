using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Tenants;

namespace RetalSystemAPI.MAUI.Views.Tenants;

public partial class TenantsPage : ContentPage
{
    public TenantsPage(TenantsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TenantsViewModel vm)
        {
            await vm.LoadTenantsCommand.ExecuteAsync(null);
        }
    }
}
