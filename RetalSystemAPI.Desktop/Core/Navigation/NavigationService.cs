using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace RetalSystemAPI.Desktop.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    public event EventHandler<UserControl>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TView>() where TView : UserControl
    {
        var view = _serviceProvider.GetRequiredService<TView>();
        Navigated?.Invoke(this, view);
    }

    public void NavigateTo(UserControl view)
    {
        Navigated?.Invoke(this, view);
    }

    public void NavigateToLogin()
    {
        // عندما يُطلب الانشغال لشاشة تسجيل الدخول
        var app = (App)System.Windows.Application.Current;
        app.ShowLoginWindow();
    }
}
