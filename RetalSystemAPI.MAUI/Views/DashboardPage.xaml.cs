using System;
using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels;

namespace RetalSystemAPI.MAUI.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            await vm.LoadDashboardCommand.ExecuteAsync(null);
        }
    }

    private async void NavigateToProducts(object sender, EventArgs e) => await Shell.Current.GoToAsync("//ProductsPage");
    private async void NavigateToCategories(object sender, EventArgs e) => await Shell.Current.GoToAsync("//CategoriesPage");
    private async void NavigateToBranches(object sender, EventArgs e) => await Shell.Current.GoToAsync("//BranchesPage");
    private async void NavigateToTenants(object sender, EventArgs e) => await Shell.Current.GoToAsync("//TenantsPage");
}
