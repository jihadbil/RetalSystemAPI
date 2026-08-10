using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Catalog;

namespace RetalSystemAPI.MAUI.Views.Catalog;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoriesViewModel vm)
        {
            await vm.LoadCategoriesCommand.ExecuteAsync(null);
        }
    }
}
