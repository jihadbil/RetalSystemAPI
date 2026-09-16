using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetalSystemAPI.Desktop.Models.Pos;

namespace RetalSystemAPI.Desktop.Views.Pos;

public partial class QuantityNumpadDialog : Window
{
    private readonly CartItemModel _cartItem;
    private bool _isFirstDigit = true;

    public int ResultQuantity { get; private set; }

    public QuantityNumpadDialog(CartItemModel cartItem)
    {
        _cartItem = cartItem ?? new CartItemModel();
        ResultQuantity = _cartItem.Quantity;

        InitializeComponent();

        if (TxtProductName != null) TxtProductName.Text = _cartItem.ProductName;
        if (TxtUnitPrice != null) TxtUnitPrice.Text = $"سعر الوحدة: {_cartItem.UnitPrice:N2} د.ل";
        if (TxtQuantity != null) TxtQuantity.Text = _cartItem.Quantity.ToString();
        UpdateCalculations();

        Loaded += (s, e) =>
        {
            TxtQuantity?.Focus();
            TxtQuantity?.SelectAll();
        };
    }

    private void UpdateCalculations()
    {
        if (TxtQuantity == null || TxtCalculatedTotal == null || _cartItem == null) return;

        if (int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
        {
            ResultQuantity = qty;
            decimal total = Math.Max(0, (qty * _cartItem.UnitPrice) - _cartItem.DiscountAmount);
            TxtCalculatedTotal.Text = $"{total:N2} د.ل";
        }
        else
        {
            TxtCalculatedTotal.Text = "0.00 د.ل";
        }
    }

    private void BtnDigit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Content is string digit)
        {
            if (_isFirstDigit)
            {
                TxtQuantity.Text = digit;
                _isFirstDigit = false;
            }
            else
            {
                if (TxtQuantity.Text == "0")
                {
                    TxtQuantity.Text = digit;
                }
                else
                {
                    TxtQuantity.Text += digit;
                }
            }
            UpdateCalculations();
        }
    }

    private void BtnPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag && int.TryParse(tag, out int val))
        {
            TxtQuantity.Text = val.ToString();
            _isFirstDigit = false;
            UpdateCalculations();
        }
    }

    private void BtnPlus_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtQuantity.Text, out int val))
        {
            val++;
            TxtQuantity.Text = val.ToString();
        }
        else
        {
            TxtQuantity.Text = "1";
        }
        _isFirstDigit = false;
        UpdateCalculations();
    }

    private void BtnMinus_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtQuantity.Text, out int val) && val > 1)
        {
            val--;
            TxtQuantity.Text = val.ToString();
        }
        _isFirstDigit = false;
        UpdateCalculations();
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        TxtQuantity.Text = "1";
        _isFirstDigit = true;
        UpdateCalculations();
    }

    private void BtnBackspace_Click(object sender, RoutedEventArgs e)
    {
        if (TxtQuantity.Text.Length > 1)
        {
            TxtQuantity.Text = TxtQuantity.Text.Substring(0, TxtQuantity.Text.Length - 1);
        }
        else
        {
            TxtQuantity.Text = "1";
            _isFirstDigit = true;
        }
        UpdateCalculations();
    }

    private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateCalculations();
    }

    private void BtnConfirm_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(TxtQuantity.Text, out int qty) && qty > 0)
        {
            ResultQuantity = qty;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("يرجى إدخال كمية صحيحة أكبر من صفر.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            BtnConfirm_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            BtnCancel_Click(sender, e);
            e.Handled = true;
        }
    }
}
