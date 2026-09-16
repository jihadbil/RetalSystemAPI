using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RetalSystemAPI.Desktop.ViewModels.Base;

public partial class BaseViewModel : ObservableValidator
{
    public virtual void SaveWorkspace() { }
    public void RefreshAppearance() => OnPropertyChanged(string.Empty);
    protected bool ValidateForm()
    {
        if (IsLoading) return false;
        ValidateAllProperties();
        if (!HasErrors) { ErrorMessage = null; return true; }
        ErrorMessage = GetErrors().FirstOrDefault()?.ErrorMessage ?? "راجع الحقول الموضحة قبل الحفظ.";
        return false;
    }
    private int _pendingOperations;
    private int _searchVersion;

    protected async Task DebounceSearchAsync(Func<Task> action)
    {
        var version = ++_searchVersion;
        await Task.Delay(300);
        if (version == _searchVersion) await action();
    }
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    protected async Task ExecuteAsync(Func<Task> action, string? loadingMessage = null, Func<bool>? isCurrent = null)
    {
        _pendingOperations++;
        IsLoading = true;
        ErrorMessage = null;
        SuccessMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            if (isCurrent?.Invoke() == false) return;
            ErrorMessage = ex is System.Net.Http.HttpRequestException or TaskCanceledException
                ? "تعذر الاتصال بالخادم. تحقق من الاتصال وأعد المحاولة؛ المدخلات ما زالت محفوظة في الشاشة."
                : ex is InvalidOperationException ? ex.Message : "تعذر إكمال العملية. احتفظ بمدخلاتك وأعد المحاولة.";
        }
        finally
        {
            IsLoading = --_pendingOperations > 0;
        }
    }
}
