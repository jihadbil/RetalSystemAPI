using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RetalSystemAPI.Desktop.Models.Pos;

public partial class CartItemModel : ObservableObject
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? ProductBarCodeId { get; set; }
    public string? BarCode { get; set; }
    public string? ImageUrl { get; set; }
    public string? UnitName { get; set; } = "قطعة";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private decimal _unitPrice;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int _quantity = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private decimal _discountAmount = 0;

    public decimal LineTotal => Math.Max(0, (Quantity * UnitPrice) - DiscountAmount);
}

public class CategoryFilterItem
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class QuickProductItem
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public string? BarCode { get; set; }
}

