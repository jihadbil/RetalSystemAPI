using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Branch;

namespace RetalSystemAPI.MAUI.ViewModels.Branches;

public partial class BranchesViewModel : ObservableObject
{
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private ObservableCollection<BranchResponseDto> _branches = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public BranchesViewModel(IBranchApiService branchApiService)
    {
        _branchApiService = branchApiService;
    }

    [RelayCommand]
    private async Task LoadBranchesAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await _branchApiService.GetAllAsync();
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Branches = new ObservableCollection<BranchResponseDto>(result.Data);
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل قائمة الفروع";
        }
    }

    [RelayCommand]
    private async Task ToggleActiveAsync(BranchResponseDto branch)
    {
        if (branch == null) return;

        var result = await _branchApiService.ToggleActiveStatusAsync(branch.Id);
        if (result.Success)
        {
            await LoadBranchesAsync();
        }
    }
}
