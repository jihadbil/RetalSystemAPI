using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Services.Customers;

namespace RetalSystemAPI.Desktop.Views.Pos;

public partial class QuickCustomerDialog : Window
{
    private readonly ICustomerApiService _customerApiService;

    public CustomerSummaryDto? CreatedCustomer { get; private set; }

    public QuickCustomerDialog(ICustomerApiService customerApiService)
    {
        InitializeComponent();
        _customerApiService = customerApiService;

        Loaded += (s, e) =>
        {
            TxtName.Focus();
        };
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtName.Text.Trim();
        var phone = TxtPhone.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("يرجى إدخال اسم العميل.");
            TxtName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            ShowError("يرجى إدخال رقم الهاتف.");
            TxtPhone.Focus();
            return;
        }

        decimal.TryParse(TxtCreditLimit.Text.Trim(), out decimal creditLimit);
        var type = CustomerType.Regular;
        if (CmbType.SelectedItem is ComboBoxItem item && item.Tag is CustomerType custType)
        {
            type = custType;
        }

        var req = new CreateCustomerRequest
        {
            Name = name,
            Code = string.IsNullOrWhiteSpace(TxtCode.Text) ? null : TxtCode.Text.Trim(),
            Address = string.IsNullOrWhiteSpace(TxtAddress.Text) ? null : TxtAddress.Text.Trim(),
            Type = type,
            CreditLimit = creditLimit,
            Phones = new List<CreateCustomerPhoneRequest>
            {
                new() { PhoneNumber = phone, ContactName = name, IsDefault = true }
            }
        };

        BtnSave.IsEnabled = false;
        TxtError.Visibility = Visibility.Collapsed;

        try
        {
            var res = await _customerApiService.CreateAsync(req);
            if (res.Success && res.Data != null)
            {
                CreatedCustomer = new CustomerSummaryDto
                {
                    Id = res.Data.Id,
                    Name = res.Data.Name,
                    Code = res.Data.Code,
                    Address = res.Data.Address,
                    Type = res.Data.Type,
                    CreditLimit = res.Data.CreditLimit,
                    CurrentBalance = res.Data.CurrentBalance,
                    IsActive = res.Data.IsActive,
                    PrimaryPhone = phone,
                    PhoneCount = 1
                };

                DialogResult = true;
                Close();
            }
            else
            {
                ShowError(res.Message ?? "فشل حفظ العميل، يرجى المحاولة مرة أخرى.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"حدث خطأ: {ex.Message}");
        }
        finally
        {
            BtnSave.IsEnabled = true;
        }
    }

    private void ShowError(string msg)
    {
        TxtError.Text = msg;
        TxtError.Visibility = Visibility.Visible;
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
            BtnSave_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            BtnCancel_Click(sender, e);
            e.Handled = true;
        }
    }
}
