using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, UserControl> _views = new();
    private UserControl? _current;
    private string? _scope;
    public event EventHandler<UserControl>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TView>() where TView : UserControl
    {
        var auth = _serviceProvider.GetRequiredService<AuthStateService>();
        var scope = $"{auth.CurrentTenantId}:{auth.CurrentUserId}";
        if (_scope != scope) { _views.Clear(); _current = null; _scope = scope; }
        if (!_views.TryGetValue(typeof(TView), out var view))
            _views[typeof(TView)] = view = _serviceProvider.GetRequiredService<TView>();
        NavigateTo(view);
    }

    public void NavigateTo(UserControl view)
    {
        SaveWorkspace();
        _current = view;
        Navigated?.Invoke(this, view);
    }

    public void SaveWorkspace()
    {
        foreach (var view in _views.Values)
            if (view.DataContext is BaseViewModel vm) vm.SaveWorkspace();
    }

    public void ResetSession()
    {
        _views.Clear();
        _current = null;
        _scope = null;
    }

    public void RefreshAppearance()
    {
        foreach (var view in _views.Values)
            if (view.DataContext is BaseViewModel vm) vm.RefreshAppearance();
    }

    public void NavigateToLogin()
    {
        // عندما يُطلب الانشغال لشاشة تسجيل الدخول
        var app = (App)System.Windows.Application.Current;
        ResetSession();
        app.ShowLoginWindow();
    }
}
