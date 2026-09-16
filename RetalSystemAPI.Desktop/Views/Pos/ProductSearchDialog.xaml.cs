using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RetalSystemAPI.Desktop.Models.Catalog;

namespace RetalSystemAPI.Desktop.Views.Pos;

public partial class ProductSearchDialog : Window
{
    private readonly List<ProductDto> _allProducts;
    private readonly List<CategoryDto> _categories;
    private List<ProductDto> _filteredProducts = new();

    public ProductDto? SelectedProduct { get; private set; }

    public ProductSearchDialog(List<ProductDto> products, List<CategoryDto>? categories = null)
    {
        _allProducts = products ?? new List<ProductDto>();
        _categories = categories ?? new List<CategoryDto>();

        InitializeComponent();

        PopulateCategories();
        ApplyFilter();

        Loaded += (s, e) =>
        {
            TxtSearch?.Focus();
        };
    }

    private void PopulateCategories()
    {
        if (CmbCategory == null) return;
        var list = new List<CategoryDto>
        {
            new() { Id = Guid.Empty, Name = "جميع الفئات" }
        };
        list.AddRange(_categories);
        CmbCategory.ItemsSource = list;
        CmbCategory.SelectedIndex = 0;
    }

    private void ApplyFilter()
    {
        if (_allProducts == null || DgProducts == null) return;

        var query = _allProducts.AsEnumerable();

        if (CmbCategory.SelectedItem is CategoryDto cat && cat.Id != Guid.Empty)
        {
            query = query.Where(p => p.CategoryId == cat.Id);
        }

        var search = TxtSearch?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                (p.Name != null && p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.Code != null && p.Code.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.DefaultBarCode != null && p.DefaultBarCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.BarCodes != null && p.BarCodes.Any(b =>
                    (b?.BarCode != null && b.BarCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (b?.Title != null && b.Title.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (b?.Description != null && b.Description.Contains(search, StringComparison.OrdinalIgnoreCase)))));
        }

        _filteredProducts = query.ToList();
        DgProducts.ItemsSource = _filteredProducts;
        if (TxtCount != null)
        {
            TxtCount.Text = $"عدد النتائج: {_filteredProducts.Count} صنف";
        }

        if (_filteredProducts.Count > 0)
        {
            DgProducts.SelectedIndex = 0;
        }
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilter();
    }

    private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        ConfirmSelection();
    }

    private void BtnSelectRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ProductDto product)
        {
            SelectedProduct = product;
            DialogResult = true;
            Close();
        }
        else
        {
            ConfirmSelection();
        }
    }

    private void ConfirmSelection()
    {
        if (DgProducts.SelectedItem is ProductDto product)
        {
            SelectedProduct = product;
            DialogResult = true;
            Close();
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ConfirmSelection();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            BtnClose_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Down && TxtSearch.IsFocused && DgProducts.Items.Count > 0)
        {
            DgProducts.Focus();
            if (DgProducts.SelectedIndex < 0)
            {
                DgProducts.SelectedIndex = 0;
            }
            e.Handled = true;
        }
    }
}
