using Microsoft.Maui.Controls;
using RetalSystemAPI.MAUI.ViewModels.Branches;

namespace RetalSystemAPI.MAUI.Views.Branches;

public partial class BranchesPage : ContentPage
{
    public BranchesPage(BranchesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BranchesViewModel vm)
        {
            await vm.LoadBranchesCommand.ExecuteAsync(null);
        }
    }
}
