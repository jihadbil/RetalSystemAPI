using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.MAUI.Services;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;

namespace RetalSystemAPI.MAUI.ViewModels.Catalog;

public partial class UnitsViewModel : ObservableObject
{
    private readonly IUnitApiService _unitApiService;

    [ObservableProperty]
    private ObservableCollection<UnitResponseDto> _units = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public UnitsViewModel(IUnitApiService unitApiService)
    {
        _unitApiService = unitApiService;
    }

    [RelayCommand]
    private async Task LoadUnitsAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        var result = await _unitApiService.GetAllAsync();
        IsBusy = false;

        if (result.Success && result.Data != null)
        {
            Units = new ObservableCollection<UnitResponseDto>(result.Data);
        }
        else
        {
            ErrorMessage = result.Message ?? "فشل تحميل وحدات القياس";
        }
    }
}
