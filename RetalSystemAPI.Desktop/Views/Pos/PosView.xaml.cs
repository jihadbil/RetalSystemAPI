using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Models.Pos;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.ViewModels.Pos;

namespace RetalSystemAPI.Desktop.Views.Pos;

public partial class PosView : UserControl
{
    private readonly PosViewModel _viewModel;
    private readonly ICustomerApiService _customerApiService;

    public PosView(PosViewModel viewModel, ICustomerApiService customerApiService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _customerApiService = customerApiService;
        DataContext = _viewModel;
        _viewModel.ConfirmActionHandler = (title, message) => Controls.ModernConfirmDialog.ShowConfirm(Window.GetWindow(this), title, message, "إفراغ السلة", "الاحتفاظ بالسلة");
        Unloaded += (_, _) => _viewModel.SaveWorkspace();
        SizeChanged += (_, _) => UpdateLayoutMode();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PosViewModel.SelectedPaymentMethod)) UpdatePaymentFields();
        };
        UpdatePaymentFields();

        // Wire Dialog Handlers
        _viewModel.ShowProductSearchDialogHandler = ShowProductSearchDialogAsync;
        _viewModel.ShowQuickCustomerDialogHandler = ShowQuickCustomerDialogAsync;
        _viewModel.ShowQuantityNumpadDialogHandler = ShowQuantityNumpadDialogAsync;

        _viewModel.RequestBarcodeFocus += () =>
        {
            Dispatcher.Invoke(() =>
            {
                BarcodeTextBox.Focus();
                BarcodeTextBox.SelectAll();
            });
        };

        Loaded += (s, e) =>
        {
            BarcodeTextBox.Focus();
            BarcodeTextBox.SelectAll();
        };
    }

    private void UpdatePaymentFields()
    {
        var method = _viewModel.SelectedPaymentMethod;
        PayCashRadio.IsChecked = method == PaymentMethod.Cash;
        PayCardRadio.IsChecked = method == PaymentMethod.Card;
        PayCreditRadio.IsChecked = method == PaymentMethod.Credit;
        CashFields.Visibility = method == PaymentMethod.Cash ? Visibility.Visible : Visibility.Collapsed;
        CashSummary.Visibility = method == PaymentMethod.Cash ? Visibility.Visible : Visibility.Collapsed;
        CreditFields.Visibility = method == PaymentMethod.Credit ? Visibility.Visible : Visibility.Collapsed;
        CardHint.Visibility = method == PaymentMethod.Card ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateLayoutMode()
    {
        var compact = ActualWidth < 780;
        PaymentColumn.Width = new GridLength(compact ? 0 : 320);
        CartRow.Height = compact ? new GridLength(400) : new GridLength(1, GridUnitType.Star);
        PaymentRow.Height = compact ? new GridLength(520) : new GridLength(0);
        Grid.SetColumn(PaymentPanel, compact ? 0 : 1);
        Grid.SetRow(PaymentPanel, compact ? 1 : 0);
        CartPanel.Margin = compact ? new Thickness(0, 0, 0, 12) : new Thickness(0, 0, 14, 0);
        SaleLayout.Height = compact ? 920 : Math.Max(380, ActualHeight - 190);
    }

    private Task<ProductDto?> ShowProductSearchDialogAsync(List<ProductDto> products, List<CategoryDto> categories)
    {
        var tcs = new TaskCompletionSource<ProductDto?>();
        Dispatcher.Invoke(() =>
        {
            var parentWin = Window.GetWindow(this);
            var dialog = new ProductSearchDialog(products, categories)
            {
                Owner = parentWin
            };

            if (dialog.ShowDialog() == true)
            {
                tcs.SetResult(dialog.SelectedProduct);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return tcs.Task;
    }

    private Task<CustomerSummaryDto?> ShowQuickCustomerDialogAsync()
    {
        var tcs = new TaskCompletionSource<CustomerSummaryDto?>();
        Dispatcher.Invoke(() =>
        {
            var parentWin = Window.GetWindow(this);
            var dialog = new QuickCustomerDialog(_customerApiService)
            {
                Owner = parentWin
            };

            if (dialog.ShowDialog() == true)
            {
                tcs.SetResult(dialog.CreatedCustomer);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return tcs.Task;
    }

    private Task<int?> ShowQuantityNumpadDialogAsync(CartItemModel item)
    {
        var tcs = new TaskCompletionSource<int?>();
        Dispatcher.Invoke(() =>
        {
            var parentWin = Window.GetWindow(this);
            var dialog = new QuantityNumpadDialog(item)
            {
                Owner = parentWin
            };

            if (dialog.ShowDialog() == true)
            {
                tcs.SetResult(dialog.ResultQuantity);
            }
            else
            {
                tcs.SetResult(null);
            }
        });
        return tcs.Task;
    }

    private void PosView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_viewModel.IsLoading) return;
        switch (e.Key)
        {
            case Key.F1:
                BarcodeTextBox.Focus();
                BarcodeTextBox.SelectAll();
                e.Handled = true;
                break;

            case Key.F2:
                if (_viewModel.OpenProductSearchDialogCommand.CanExecute(null))
                {
                    _viewModel.OpenProductSearchDialogCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.F3:
                if (_viewModel.OpenQuickCustomerDialogCommand.CanExecute(null))
                {
                    _viewModel.OpenQuickCustomerDialogCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.F4:
                if (_viewModel.ToggleQuickItemsCommand.CanExecute(null))
                {
                    _viewModel.ToggleQuickItemsCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.F5:
                if (_viewModel.HoldOrderCommand.CanExecute(null))
                {
                    _viewModel.HoldOrderCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.F6:
                if (_viewModel.HasHeldOrder && _viewModel.RestoreHeldOrderCommand.CanExecute(null))
                {
                    _viewModel.RestoreHeldOrderCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.F8:
                if (_viewModel.SetQuickCashCommand.CanExecute("0"))
                {
                    _viewModel.SetQuickCashCommand.Execute("0");
                    e.Handled = true;
                }
                break;

            case Key.F9:
                _viewModel.SelectedPaymentMethod = PaymentMethod.Card;
                PayCardRadio.IsChecked = true;
                e.Handled = true;
                break;

            case Key.F10:
                if (Helpers.WorkspaceWindowBehavior.FindInvalidField(this) is { } invalid)
                {
                    invalid.Focus(); invalid.BringIntoView(); e.Handled = true; return;
                }
                if (_viewModel.SubmitSaleCommand.CanExecute(null))
                {
                    _viewModel.SubmitSaleCommand.Execute(null);
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                // Let the focused control close its popup or cancel editing normally.
                break;
        }
    }

    private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (_viewModel.IsLoading) { e.Handled = true; return; }
        if (e.Key == Key.Enter)
        {
            if (_viewModel.AddBarcodeCommand.CanExecute(null))
            {
                _viewModel.AddBarcodeCommand.Execute(null);
            }
            BarcodeTextBox.Focus();
            BarcodeTextBox.SelectAll();
            e.Handled = true;
        }
    }

    private void PayCash_Checked(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.Cash;
        }
    }

    private void PayCard_Checked(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.Card;
        }
    }

    private void PayCredit_Checked(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SelectedPaymentMethod = PaymentMethod.Credit;
        }
    }

    private CartItemModel? _activePopupCartItem;
    private bool _popupTextResetOnNextDigit = true;

    private void QuantityButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is CartItemModel item)
        {
            _activePopupCartItem = item;
            _popupTextResetOnNextDigit = true;
            PopupItemName.Text = item.ProductName;
            PopupUnitPrice.Text = $"{item.UnitPrice:N2} د.ل";
            PopupQuantityText.Text = item.Quantity.ToString();
            QuantityPopup.PlacementTarget = btn;
            QuantityPopup.IsOpen = true;
            PopupQuantityText.Focus();
            PopupQuantityText.SelectAll();
        }
    }

    private void PopupMinus_Click(object sender, RoutedEventArgs e)
    {
        _popupTextResetOnNextDigit = false;
        if (int.TryParse(PopupQuantityText.Text, out var qty) && qty > 1)
        {
            PopupQuantityText.Text = (qty - 1).ToString();
        }
    }

    private void PopupPlus_Click(object sender, RoutedEventArgs e)
    {
        _popupTextResetOnNextDigit = false;
        if (int.TryParse(PopupQuantityText.Text, out var qty))
        {
            PopupQuantityText.Text = (qty + 1).ToString();
        }
        else
        {
            PopupQuantityText.Text = "1";
        }
    }

    private void PopupDigit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string digit)
        {
            if (_popupTextResetOnNextDigit || PopupQuantityText.Text == "0" || PopupQuantityText.SelectedText == PopupQuantityText.Text)
            {
                PopupQuantityText.Text = digit;
                _popupTextResetOnNextDigit = false;
            }
            else if (PopupQuantityText.Text.Length < 4)
            {
                PopupQuantityText.Text += digit;
            }
            PopupQuantityText.CaretIndex = PopupQuantityText.Text.Length;
        }
    }

    private void PopupClear_Click(object sender, RoutedEventArgs e)
    {
        PopupQuantityText.Text = "0";
        _popupTextResetOnNextDigit = false;
    }

    private void PopupBackspace_Click(object sender, RoutedEventArgs e)
    {
        _popupTextResetOnNextDigit = false;
        if (PopupQuantityText.Text.Length > 1)
        {
            PopupQuantityText.Text = PopupQuantityText.Text[..^1];
        }
        else
        {
            PopupQuantityText.Text = "0";
        }
    }

    private void PopupConfirm_Click(object sender, RoutedEventArgs e)
    {
        ConfirmPopupQuantity();
    }

    private void PopupQuantity_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmPopupQuantity();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            QuantityPopup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void ConfirmPopupQuantity()
    {
        if (_activePopupCartItem != null && int.TryParse(PopupQuantityText.Text, out var newQty))
        {
            _viewModel.SetItemQuantity(_activePopupCartItem, newQty);
        }
        QuantityPopup.IsOpen = false;
        _activePopupCartItem = null;
        BarcodeTextBox.Focus();
        BarcodeTextBox.SelectAll();
    }
}

