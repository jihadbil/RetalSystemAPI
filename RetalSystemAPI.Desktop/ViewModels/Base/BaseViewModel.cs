using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RetalSystemAPI.Desktop.ViewModels.Base;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    protected async Task ExecuteAsync(Func<Task> action, string? loadingMessage = null)
    {
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
