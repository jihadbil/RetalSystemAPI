using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RetalSystemAPI.Desktop.Controls;

/// <summary>Mutually exclusive empty/error states; loading is owned by the view's overlay.</summary>
public partial class ListFeedback : UserControl
{
    public static readonly DependencyProperty ItemsCountProperty = DependencyProperty.Register(
        nameof(ItemsCount), typeof(int), typeof(ListFeedback), new PropertyMetadata(0, OnStateChanged));
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading), typeof(bool), typeof(ListFeedback), new PropertyMetadata(false, OnStateChanged));
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(ListFeedback), new PropertyMetadata(null, OnStateChanged));
    public static readonly DependencyProperty RetryCommandProperty = DependencyProperty.Register(
        nameof(RetryCommand), typeof(ICommand), typeof(ListFeedback));
    public static readonly DependencyProperty EmptyDescriptionProperty = DependencyProperty.Register(
        nameof(EmptyDescription), typeof(string), typeof(ListFeedback),
        new PropertyMetadata("جرّب تعديل البحث أو عوامل التصفية، أو أضف سجلًا جديدًا من أعلى الشاشة."));

    public int ItemsCount { get => (int)GetValue(ItemsCountProperty); set => SetValue(ItemsCountProperty, value); }
    public bool IsLoading { get => (bool)GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public string? Message { get => (string?)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public ICommand? RetryCommand { get => (ICommand?)GetValue(RetryCommandProperty); set => SetValue(RetryCommandProperty, value); }
    public string EmptyDescription { get => (string)GetValue(EmptyDescriptionProperty); set => SetValue(EmptyDescriptionProperty, value); }

    public ListFeedback()
    {
        InitializeComponent();
        UpdateState();
    }

    private static void OnStateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((ListFeedback)sender).UpdateState();

    private void UpdateState()
    {
        if (ErrorPanel is null || EmptyPanel is null) return;
        var failed = !string.IsNullOrWhiteSpace(Message);
        Visibility = !IsLoading && (failed || ItemsCount == 0) ? Visibility.Visible : Visibility.Collapsed;
        ErrorPanel.Visibility = failed ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = failed ? Visibility.Collapsed : Visibility.Visible;
    }
}
