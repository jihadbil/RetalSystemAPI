using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using RetalSystemAPI.Desktop.Services;

namespace RetalSystemAPI.Desktop.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v) return v == Visibility.Visible;
        return false;
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility v) return v != Visibility.Visible;
        return true;
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool active && active)
        {
            return new SolidColorBrush(Color.FromRgb(0x38, 0xE0, 0x7B)); // Emerald Green
        }
        return new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80)); // Muted Gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool active && active) return "نشط";
        return "غير نشط";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StringNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
            return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class NumberToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count && count > 0) return Visibility.Visible;
        if (value is long lCount && lCount > 0) return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StepVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter != null)
        {
            string paramStr = parameter.ToString() ?? "";
            if (paramStr.Contains("_"))
            {
                var parts = paramStr.Split('_');
                foreach (var part in parts)
                {
                    if (int.TryParse(part, out int target) && currentStep == target)
                        return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            if (int.TryParse(paramStr, out int expectedStep))
            {
                return currentStep == expectedStep ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StepNumberToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentStep && parameter != null && int.TryParse(parameter.ToString(), out int targetStep))
        {
            if (currentStep >= targetStep)
            {
                return new SolidColorBrush(Color.FromRgb(0x4F, 0x46, 0xE5));
            }
        }
        return new SolidColorBrush(Color.FromRgb(0x47, 0x55, 0x69));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ToastTypeToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastType type)
        {
            return type switch
            {
                ToastType.Success => new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
                ToastType.Error => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
                ToastType.Warning => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
                _ => new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB))
            };
        }
        return new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ToastTypeToLightBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastType type)
        {
            return type switch
            {
                ToastType.Success => new SolidColorBrush(Color.FromRgb(0xEC, 0xFD, 0xF5)),
                ToastType.Error => new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2)),
                ToastType.Warning => new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xEB)),
                _ => new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF))
            };
        }
        return new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ToastTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var defaultIcon = Application.Current.FindResource("IconInfo") as Geometry ?? Geometry.Empty;
        if (value is ToastType type)
        {
            return type switch
            {
                ToastType.Success => Application.Current.FindResource("IconCheck") as Geometry ?? defaultIcon,
                ToastType.Error => Application.Current.FindResource("IconDelete") as Geometry ?? defaultIcon,
                ToastType.Warning => Application.Current.FindResource("IconAlert") as Geometry ?? defaultIcon,
                _ => defaultIcon
            };
        }
        return defaultIcon;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ActiveNavToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeSection && parameter is string targetSection)
        {
            if (string.Equals(activeSection, targetSection, StringComparison.OrdinalIgnoreCase))
            {
                return Application.Current.FindResource("PrimaryLightBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0xE8, 0xFB, 0xF0));
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ActiveNavToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeSection && parameter is string targetSection)
        {
            if (string.Equals(activeSection, targetSection, StringComparison.OrdinalIgnoreCase))
            {
                return Application.Current.FindResource("PrimaryDarkBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x0F, 0x29, 0x1E));
            }
        }
        return Application.Current.FindResource("TextPrimaryBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ActiveNavToBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string activeSection && parameter is string targetSection)
        {
            if (string.Equals(activeSection, targetSection, StringComparison.OrdinalIgnoreCase))
            {
                return Application.Current.FindResource("PrimaryBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x38, 0xE0, 0x7B));
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TabIndexToActiveBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
        {
            if (currentIndex == targetIndex)
            {
                return Application.Current.FindResource("PrimaryLightBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0xEE, 0xF2, 0xFF));
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TabIndexToActiveForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
        {
            if (currentIndex == targetIndex)
            {
                return Application.Current.FindResource("PrimaryBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x4F, 0x46, 0xE5));
            }
        }
        return Application.Current.FindResource("TextSecondaryBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TabIndexToActiveBorderBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currentIndex && parameter != null && int.TryParse(parameter.ToString(), out int targetIndex))
        {
            if (currentIndex == targetIndex)
            {
                return Application.Current.FindResource("PrimaryBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x4F, 0x46, 0xE5));
            }
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
