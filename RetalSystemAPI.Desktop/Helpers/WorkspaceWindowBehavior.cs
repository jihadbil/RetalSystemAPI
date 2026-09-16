using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RetalSystemAPI.Desktop.Controls;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.Helpers;

/// <summary>Shared sizing, keyboard validation and close protection for editor windows.</summary>
public static class WorkspaceWindowBehavior
{
    public static void Register()
    {
        EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnLoaded));
        EventManager.RegisterClassHandler(typeof(Button), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((sender, e) =>
        {
            if (sender is Button button && !CanSubmit(button)) e.Handled = true;
        }));
        EventManager.RegisterClassHandler(typeof(Button), UIElement.PreviewKeyDownEvent, new KeyEventHandler((sender, e) =>
        {
            if (e.Key is Key.Enter or Key.Space && sender is Button button && !CanSubmit(button)) e.Handled = true;
        }));
    }

    private static bool CanSubmit(Button button)
    {
        if (!IsSaveButton(button)) return true;
        if (FindInvalidField(Window.GetWindow(button) ?? (DependencyObject)button) is not { } invalid) return true;
        invalid.Focus(); invalid.BringIntoView(); return false;
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window || e.OriginalSource != window) return;
        window.UseLayoutRounding = true;
        var area = SystemParameters.WorkArea;
        window.MinWidth = Math.Min(window.MinWidth, Math.Max(320, area.Width - 24));
        window.MinHeight = Math.Min(window.MinHeight, Math.Max(300, area.Height - 24));
        window.MaxWidth = area.Width;
        window.MaxHeight = area.Height;
        if (window.Width > area.Width) window.Width = area.Width;
        if (window.Height > area.Height) window.Height = area.Height;
        window.Left = Math.Clamp(window.Left, area.Left, Math.Max(area.Left, area.Right - window.ActualWidth));
        window.Top = Math.Clamp(window.Top, area.Top, Math.Max(area.Top, area.Bottom - window.ActualHeight));

        var isEditor = window.GetType().Name.Contains("Form") || window.GetType().Name == "ResetPasswordWindow" || window.GetType().Name == "QuickCustomerDialog";
        if (!isEditor) return;
        window.ResizeMode = ResizeMode.CanResizeWithGrip;
        window.SetResourceReference(Control.FontFamilyProperty, "AppFontFamily");
        // A bounded outer scroller keeps save/cancel reachable on small work areas.
        if (window.Content is FrameworkElement content && content is not ScrollViewer)
        {
            var minimumHeight = Math.Min(Math.Max(window.ActualHeight - 70, 300), 660);
            window.Content = null;
            content.MinHeight = minimumHeight;
            content.MinWidth = Math.Min(Math.Max(window.ActualWidth - 60, 300), 720);
            window.Content = new ScrollViewer
            {
                Content = content, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }
        var dirty = false;
        if (window.DataContext is BaseViewModel editor)
        {
            void ReflectBusyState(object? _, PropertyChangedEventArgs args)
            {
                if (args.PropertyName != nameof(BaseViewModel.IsLoading)) return;
                if (window.Content is UIElement body) body.IsEnabled = !editor.IsLoading;
                window.Cursor = editor.IsLoading ? Cursors.Wait : Cursors.Arrow;
            }
            editor.PropertyChanged += ReflectBusyState;
            ReflectBusyState(editor, new PropertyChangedEventArgs(nameof(BaseViewModel.IsLoading)));
            window.Closed += (_, _) => editor.PropertyChanged -= ReflectBusyState;
        }
        window.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler((_, args) =>
        {
            if (args.OriginalSource is TextBox box && box.IsKeyboardFocusWithin && !box.IsReadOnly) dirty = true;
        }));
        window.AddHandler(PasswordBox.PasswordChangedEvent, new RoutedEventHandler((_, args) =>
        {
            if (args.OriginalSource is PasswordBox box && box.IsKeyboardFocusWithin) dirty = true;
        }));
        window.AddHandler(Selector.SelectionChangedEvent, new SelectionChangedEventHandler((_, args) =>
        {
            if (args.OriginalSource is ComboBox combo && (combo.IsKeyboardFocusWithin || combo.IsMouseOver)) dirty = true;
        }));
        window.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((_, args) =>
        {
            if (args.OriginalSource is CheckBox or RadioButton) dirty = true;
        }));
        window.Closing += (_, args) =>
        {
            if (window.DialogResult == true) return;
            if (window.DataContext is BaseViewModel { IsLoading: true })
            {
                args.Cancel = true;
                return;
            }
            if (dirty && !ModernConfirmDialog.ShowConfirm(window, "إغلاق النموذج", "قد توجد تعديلات لم تُحفظ. هل تريد إغلاق النموذج؟", "إغلاق النموذج", "متابعة التحرير", ConfirmDialogType.Warning))
                args.Cancel = true;
        };
        AddFieldNames(window);
    }

    private static bool IsSaveButton(Button button)
    {
        var path = System.Windows.Data.BindingOperations.GetBinding(button, Button.CommandProperty)?.Path?.Path;
        return path is "SaveCommand" or "SubmitSaleCommand";
    }

    public static FrameworkElement? FindInvalidField(DependencyObject root)
    {
        if (root is FrameworkElement field && field.IsVisible && Validation.GetHasError(field)) return field;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            if (FindInvalidField(VisualTreeHelper.GetChild(root, i)) is { } invalid) return invalid;
        return null;
    }

    private static void AddFieldNames(DependencyObject root)
    {
        if (root is StackPanel panel)
        {
            string? label = null;
            foreach (var child in panel.Children)
            {
                if (child is TextBlock text) label = text.Text;
                else if (child is Control control && !string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(AutomationProperties.GetName(control)))
                    AutomationProperties.SetName(control, label);
            }
        }
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) AddFieldNames(VisualTreeHelper.GetChild(root, i));
    }
}
