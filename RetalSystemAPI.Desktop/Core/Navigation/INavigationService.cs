using System;
using System.Windows.Controls;

namespace RetalSystemAPI.Desktop.Core.Navigation;

public interface INavigationService
{
    void NavigateTo<TView>() where TView : UserControl;
    void NavigateTo(UserControl view);
    void NavigateToLogin();
    event EventHandler<UserControl>? Navigated;
}
