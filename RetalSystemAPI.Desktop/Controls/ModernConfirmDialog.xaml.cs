using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RetalSystemAPI.Desktop.Controls;

public enum ConfirmDialogType
{
    Danger,
    Warning,
    Info
}

public partial class ModernConfirmDialog : Window
{
    public bool IsConfirmed { get; private set; }

    public ModernConfirmDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => CancelButton.Focus();
    }

    public static bool ShowConfirm(Window owner, string title, string message, string confirmText = "تأكيد", string cancelText = "إلغاء", ConfirmDialogType dialogType = ConfirmDialogType.Danger)
    {
        var dialog = new ModernConfirmDialog
        {
            Owner = owner
        };

        dialog.TitleTextBlock.Text = title;
        dialog.MessageTextBlock.Text = message;
        dialog.ConfirmButton.Content = confirmText;
        dialog.CancelButton.Content = cancelText;

        if (dialogType == ConfirmDialogType.Danger)
        {
            dialog.IconBadge.SetResourceReference(System.Windows.Controls.Border.BackgroundProperty, "DangerLightBrush");
            dialog.IconText.Text = "🗑️";
            dialog.ConfirmButton.Style = (Style)dialog.FindResource("DangerButtonStyle");
        }
        else if (dialogType == ConfirmDialogType.Warning)
        {
            dialog.IconBadge.SetResourceReference(System.Windows.Controls.Border.BackgroundProperty, "WarningLightBrush");
            dialog.IconText.Text = "⚠️";
            dialog.ConfirmButton.Style = (Style)dialog.FindResource("PrimaryButtonStyle");
        }
        else
        {
            dialog.IconBadge.SetResourceReference(System.Windows.Controls.Border.BackgroundProperty, "AccentLightBrush");
            dialog.IconText.Text = "ℹ️";
            dialog.ConfirmButton.Style = (Style)dialog.FindResource("PrimaryButtonStyle");
        }

        dialog.ShowDialog();
        return dialog.IsConfirmed;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = true;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        DialogResult = false;
        Close();
    }
}
