using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RetalSystemAPI.Desktop.Controls;

public partial class PaginationControl : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(PaginationControl), new PropertyMetadata(1, OnPageChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(nameof(TotalPages), typeof(int), typeof(PaginationControl), new PropertyMetadata(1, OnPageChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PaginationControl),
            new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageSizeChanged));

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(PaginationControl), new PropertyMetadata(false, OnPageChanged));

    public static readonly DependencyProperty NextPageCommandProperty =
        DependencyProperty.Register(nameof(NextPageCommand), typeof(ICommand), typeof(PaginationControl));

    public static readonly DependencyProperty PrevPageCommandProperty =
        DependencyProperty.Register(nameof(PrevPageCommand), typeof(ICommand), typeof(PaginationControl));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public int[] PageSizeOptions { get; } = new[] { 5, 10, 20, 50, 100 };

    public bool CanGoNext => !IsLoading && CurrentPage < TotalPages;
    public bool CanGoPrev => !IsLoading && CurrentPage > 1;
    public bool CanChangePageSize => !IsLoading;

    public ICommand NextPageCommand
    {
        get => (ICommand)GetValue(NextPageCommandProperty);
        set => SetValue(NextPageCommandProperty, value);
    }

    public ICommand PrevPageCommand
    {
        get => (ICommand)GetValue(PrevPageCommandProperty);
        set => SetValue(PrevPageCommandProperty, value);
    }

    public string PageInfoText => $"الصفحة {CurrentPage} من {TotalPages}";

    public PaginationControl()
    {
        InitializeComponent();
    }

    private static void OnPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.OnPropertyChanged(nameof(PageInfoText));
            control.OnPropertyChanged(nameof(CurrentPage));
            control.OnPropertyChanged(nameof(TotalPages));
            control.OnPropertyChanged(nameof(IsLoading));
            control.OnPropertyChanged(nameof(CanGoNext));
            control.OnPropertyChanged(nameof(CanGoPrev));
            control.OnPropertyChanged(nameof(CanChangePageSize));
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.OnPropertyChanged(nameof(PageSize));
        }
    }

    private void Prev_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoPrev && PrevPageCommand?.CanExecute(null) == true) PrevPageCommand.Execute(null);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (CanGoNext && NextPageCommand?.CanExecute(null) == true) NextPageCommand.Execute(null);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
