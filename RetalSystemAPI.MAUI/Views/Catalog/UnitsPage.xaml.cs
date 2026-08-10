using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Catalog;

namespace RetalSystemAPI.MAUI.Views.Catalog;

public partial class UnitsPage : ContentPage
{
    public UnitsPage(UnitsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UnitsViewModel vm)
        {
            await vm.LoadUnitsCommand.ExecuteAsync(null);
        }
    }
}
