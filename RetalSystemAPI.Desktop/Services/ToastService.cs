using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RetalSystemAPI.Desktop.Services;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public partial class ToastItem : ObservableObject
{
    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private ToastType _type = ToastType.Info;

    public Action<ToastItem>? OnDismiss { get; set; }

    [RelayCommand]
    private void Dismiss()
    {
        OnDismiss?.Invoke(this);
    }
}

public interface IToastService
{
    ObservableCollection<ToastItem> ActiveToasts { get; }
    void Show(string title, string message, ToastType type = ToastType.Info, int durationSeconds = 3);
    void ShowSuccess(string message, string title = "نجاح العملية");
    void ShowError(string message, string title = "تنبيه خطأ");
    void ShowWarning(string message, string title = "تحذير");
    void ShowInfo(string message, string title = "إشعار");
    void Dismiss(ToastItem toast);
}

public class ToastService : IToastService
{
    public ObservableCollection<ToastItem> ActiveToasts { get; } = new();

    public void Show(string title, string message, ToastType type = ToastType.Info, int durationSeconds = 3)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var item = new ToastItem
            {
                Title = title,
                Message = message,
                Type = type
            };

            item.OnDismiss = Dismiss;
            ActiveToasts.Insert(0, item);

            // Auto dismiss after timeout
            Task.Delay(TimeSpan.FromSeconds(durationSeconds)).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() => Dismiss(item));
            });
        });
    }

    public void ShowSuccess(string message, string title = "نجاح العملية")
    {
        Show(title, message, ToastType.Success);
    }

    public void ShowError(string message, string title = "تنبيه خطأ")
    {
        Show(title, message, ToastType.Error, 5);
    }

    public void ShowWarning(string message, string title = "تحذير")
    {
        Show(title, message, ToastType.Warning, 4);
    }

    public void ShowInfo(string message, string title = "إشعار")
    {
        Show(title, message, ToastType.Info);
    }

    public void Dismiss(ToastItem toast)
    {
        ActiveToasts.Remove(toast);
    }
}
