using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.ViewModels.Pos;

namespace RetalSystemAPI.Desktop.Views.Pos;

public partial class PosView : UserControl
{
    private readonly PosViewModel _viewModel;

    public PosView(PosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (_viewModel.AddBarcodeCommand.CanExecute(null))
            {
                _viewModel.AddBarcodeCommand.Execute(null);
            }
            BarcodeTextBox.Focus();
            BarcodeTextBox.SelectAll();
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
}
