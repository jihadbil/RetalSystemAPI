using System;
using System.Linq;
using System.Windows;

namespace RetalSystemAPI.Desktop.Services;

public enum AppTheme
{
    Light,
    Dark
}

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    bool IsDarkMode { get; }
    event EventHandler<AppTheme>? ThemeChanged;
    void SetTheme(AppTheme theme);
    void ToggleTheme();
}

public class ThemeService : IThemeService
{
    private static string PreferencePath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetalSystem", "theme.txt");

    public ThemeService()
    {
        try
        {
            if (System.IO.File.Exists(PreferencePath) && Enum.TryParse<AppTheme>(System.IO.File.ReadAllText(PreferencePath), out var saved)) SetTheme(saved);
        }
        catch (System.IO.IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    private const string LightThemeUri = "Resources/Themes/LightTheme.xaml";
    private const string DarkThemeUri = "Resources/Themes/DarkTheme.xaml";

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;
    public bool IsDarkMode => CurrentTheme == AppTheme.Dark;
    public event EventHandler<AppTheme>? ThemeChanged;

    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;
        var themeUri = theme == AppTheme.Dark ? DarkThemeUri : LightThemeUri;

        var appResources = Application.Current.Resources;
        var targetDict = appResources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("LightTheme.xaml") || d.Source.OriginalString.Contains("DarkTheme.xaml")));

        var newDict = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };

        if (targetDict != null)
        {
            var index = appResources.MergedDictionaries.IndexOf(targetDict);
            appResources.MergedDictionaries[index] = newDict;
        }
        else
        {
            appResources.MergedDictionaries.Insert(0, newDict);
        }

        try
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PreferencePath)!);
            System.IO.File.WriteAllText(PreferencePath, theme.ToString());
        }
        catch (System.IO.IOException) { }
        catch (UnauthorizedAccessException) { }
        ThemeChanged?.Invoke(this, CurrentTheme);
    }

    public void ToggleTheme()
    {
        SetTheme(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);
    }
}
