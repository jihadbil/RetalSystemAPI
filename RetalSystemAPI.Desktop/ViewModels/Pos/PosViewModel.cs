using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Customers;
using RetalSystemAPI.Desktop.Models.Pos;
using RetalSystemAPI.Desktop.Models.Sales;
using RetalSystemAPI.Desktop.Models.Warehouses;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Catalog;
using RetalSystemAPI.Desktop.Services.Customers;
using RetalSystemAPI.Desktop.Services.Sales;
using RetalSystemAPI.Desktop.Services.Warehouses;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Pos;

public partial class PosViewModel : BaseViewModel
{
    private readonly IBranchApiService _branchApiService;
    private readonly IWarehouseApiService _warehouseApiService;
    private readonly ICustomerApiService _customerApiService;
    private readonly ICategoryApiService _categoryApiService;
    private readonly IProductApiService _productApiService;
    private readonly ISalesInvoiceApiService _salesInvoiceApiService;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<WarehouseSummaryDto> _warehouses = new();

    [ObservableProperty]
    private WarehouseSummaryDto? _selectedWarehouse;

    [ObservableProperty]
    private ObservableCollection<CustomerSummaryDto> _customers = new();

    [ObservableProperty]
    private CustomerSummaryDto? _selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<CategoryFilterItem> _categoryTabs = new();

    [ObservableProperty]
    private CategoryFilterItem? _selectedCategoryTab;

    private List<ProductDto> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> _filteredProducts = new();

    [ObservableProperty]
    private ObservableCollection<CartItemModel> _cartItems = new();

    [ObservableProperty]
    private string _barcodeQuery = string.Empty;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _orderDiscount;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _grandTotal;

    [ObservableProperty]
    private decimal _paidAmount;

    [ObservableProperty]
    private decimal _changeAmount;

    [ObservableProperty]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private string? _orderNotes;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _hasHeldOrder;

    private List<CartItemModel>? _heldCartItems;
    private CustomerSummaryDto? _heldCustomer;

    public Func<string, string, Task<bool>>? ConfirmActionHandler { get; set; }

    public PosViewModel(
        IBranchApiService branchApiService,
        IWarehouseApiService warehouseApiService,
        ICustomerApiService customerApiService,
        ICategoryApiService categoryApiService,
        IProductApiService productApiService,
        ISalesInvoiceApiService salesInvoiceApiService)
    {
        _branchApiService = branchApiService;
        _warehouseApiService = warehouseApiService;
        _customerApiService = customerApiService;
        _categoryApiService = categoryApiService;
        _productApiService = productApiService;
        _salesInvoiceApiService = salesInvoiceApiService;

        _ = InitializeAsync();
    }

    partial void OnSearchQueryChanged(string value) => FilterProducts();

    partial void OnSelectedCategoryTabChanged(CategoryFilterItem? value)
    {
        if (value != null)
        {
            foreach (var tab in CategoryTabs)
            {
                tab.IsSelected = (tab.Id == value.Id);
            }
        }
        FilterProducts();
    }

    partial void OnSelectedBranchChanged(BranchDto? value)
    {
        _ = FilterWarehousesForBranchAsync();
    }

    partial void OnOrderDiscountChanged(decimal value) => RecalculateTotals();
    partial void OnTaxAmountChanged(decimal value) => RecalculateTotals();

    partial void OnPaidAmountChanged(decimal value)
    {
        ChangeAmount = Math.Max(0, value - GrandTotal);
    }

    public async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            // 1. Branches
            var branchRes = await _branchApiService.GetAllAsync();
            if (branchRes.Success && branchRes.Data != null)
            {
                Branches = new ObservableCollection<BranchDto>(branchRes.Data);
                if (SelectedBranch == null && Branches.Count > 0)
                {
                    SelectedBranch = Branches[0];
                }
            }

            // 2. Warehouses (Showrooms)
            await FilterWarehousesForBranchAsync();

            // 3. Customers
            var custRes = await _customerApiService.GetAllAsync(isActive: true);
            if (custRes.Success && custRes.Data != null)
            {
                Customers = new ObservableCollection<CustomerSummaryDto>(custRes.Data);
            }

            // 4. Categories Tabs
            var catRes = await _categoryApiService.GetAllAsync();
            var tabs = new List<CategoryFilterItem>
            {
                new() { Id = null, Name = "الكل", IsSelected = true }
            };
            if (catRes.Success && catRes.Data != null)
            {
                tabs.AddRange(catRes.Data.Select(c => new CategoryFilterItem { Id = c.Id, Name = c.Name }));
            }
            CategoryTabs = new ObservableCollection<CategoryFilterItem>(tabs);
            SelectedCategoryTab = tabs[0];

            // 5. Products
            var prodRes = await _productApiService.GetAllAsync();
            if (prodRes.Success && prodRes.Data != null)
            {
                _allProducts = prodRes.Data;
                FilterProducts();
            }
        });
    }

    private async Task FilterWarehousesForBranchAsync()
    {
        var whRes = await _warehouseApiService.GetAllAsync(branchId: SelectedBranch?.Id);
        if (whRes.Success && whRes.Data != null)
        {
            Warehouses = new ObservableCollection<WarehouseSummaryDto>(whRes.Data);
            if (SelectedWarehouse == null || !Warehouses.Any(w => w.Id == SelectedWarehouse.Id))
            {
                // Prefer Showroom type if available
                var showroom = Warehouses.FirstOrDefault(w => w.TypeName.Contains("عرض") || w.TypeName.Contains("Showroom"));
                SelectedWarehouse = showroom ?? Warehouses.FirstOrDefault();
            }
        }
    }

    private void FilterProducts()
    {
        var query = _allProducts.AsEnumerable();

        if (SelectedCategoryTab?.Id != null)
        {
            query = query.Where(p => p.CategoryId == SelectedCategoryTab.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var search = SearchQuery.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search) ||
                (p.BarCodes != null && p.BarCodes.Any(b => b.BarCode.ToLower().Contains(search))));
        }

        FilteredProducts = new ObservableCollection<ProductDto>(query);
    }

    [RelayCommand]
    private Task RefreshAsync() => InitializeAsync();

    [RelayCommand]
    private void SelectCategory(CategoryFilterItem? tab)
    {
        if (tab != null)
        {
            SelectedCategoryTab = tab;
        }
    }

    [RelayCommand]
    private void AddBarcode()
    {
        if (string.IsNullOrWhiteSpace(BarcodeQuery)) return;

        var code = BarcodeQuery.Trim();
        var product = _allProducts.FirstOrDefault(p =>
            p.Code.Equals(code, StringComparison.OrdinalIgnoreCase) ||
            p.Name.Equals(code, StringComparison.OrdinalIgnoreCase) ||
            (p.BarCodes != null && p.BarCodes.Any(b => b.BarCode.Equals(code, StringComparison.OrdinalIgnoreCase))));

        if (product != null)
        {
            var barCodeObj = product.BarCodes?.FirstOrDefault(b => b.BarCode.Equals(code, StringComparison.OrdinalIgnoreCase));
            AddProductToCart(product, barCodeObj?.Id, barCodeObj?.BarCode ?? code);
            BarcodeQuery = string.Empty;
            ErrorMessage = null;
        }
        else
        {
            ErrorMessage = $"لم يتم العثور على صنف بالباركود/الكود '{code}'";
        }
    }

    [RelayCommand]
    private void AddProduct(ProductDto? product)
    {
        if (product != null)
        {
            AddProductToCart(product);
        }
    }

    private void AddProductToCart(ProductDto product, Guid? barcodeId = null, string? barcodeStr = null)
    {
        var existing = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            var item = new CartItemModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Code = product.Code,
                ProductBarCodeId = barcodeId,
                BarCode = barcodeStr,
                UnitPrice = product.SalePrice,
                Quantity = 1,
                DiscountAmount = 0
            };
            item.PropertyChanged += (s, e) => RecalculateTotals();
            CartItems.Add(item);
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void IncrementQuantity(CartItemModel? item)
    {
        if (item != null)
        {
            item.Quantity++;
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void DecrementQuantity(CartItemModel? item)
    {
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                CartItems.Remove(item);
            }
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void RemoveCartItem(CartItemModel? item)
    {
        if (item != null)
        {
            CartItems.Remove(item);
            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        OrderDiscount = 0;
        TaxAmount = 0;
        PaidAmount = 0;
        ChangeAmount = 0;
        OrderNotes = string.Empty;
        SuccessMessage = null;
        ErrorMessage = null;
        RecalculateTotals();
    }

    [RelayCommand]
    private void HoldOrder()
    {
        if (CartItems.Count == 0) return;

        _heldCartItems = CartItems.ToList();
        _heldCustomer = SelectedCustomer;
        HasHeldOrder = true;

        CartItems.Clear();
        SelectedCustomer = null;
        RecalculateTotals();
        SuccessMessage = "تم تعليق الطلب بنجاح. يمكنك خدمة عميل آخر واستعادته لاحقاً.";
    }

    [RelayCommand]
    private void RestoreHeldOrder()
    {
        if (_heldCartItems != null && _heldCartItems.Count > 0)
        {
            CartItems = new ObservableCollection<CartItemModel>(_heldCartItems);
            foreach (var item in CartItems)
            {
                item.PropertyChanged += (s, e) => RecalculateTotals();
            }
            SelectedCustomer = _heldCustomer;

            _heldCartItems = null;
            _heldCustomer = null;
            HasHeldOrder = false;

            RecalculateTotals();
            SuccessMessage = "تمت استعادة الطلب المعلق.";
        }
    }

    [RelayCommand]
    private void SetQuickCash(object? parameter)
    {
        if (parameter is string valStr && decimal.TryParse(valStr, out decimal val))
        {
            if (val == 0)
            {
                PaidAmount = GrandTotal;
            }
            else
            {
                PaidAmount = val;
            }
        }
        else if (parameter is decimal num)
        {
            PaidAmount = (num == 0) ? GrandTotal : num;
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = CartItems.Sum(c => c.LineTotal);
        GrandTotal = Math.Max(0, SubTotal - OrderDiscount + TaxAmount);

        if (PaidAmount < GrandTotal && SelectedPaymentMethod == PaymentMethod.Cash)
        {
            PaidAmount = GrandTotal;
        }

        ChangeAmount = Math.Max(0, PaidAmount - GrandTotal);
    }

    [RelayCommand]
    private async Task SubmitSaleAsync()
    {
        if (SelectedBranch == null)
        {
            ErrorMessage = "يجب اختيار الفرع أولاً";
            return;
        }

        if (SelectedWarehouse == null)
        {
            ErrorMessage = "يجب اختيار صالة العرض أو المخزن للخصم منه";
            return;
        }

        if (CartItems.Count == 0)
        {
            ErrorMessage = "سلة المبيعات فارغة! يرجى إضافة أصناف للطلب.";
            return;
        }

        ErrorMessage = null;
        SuccessMessage = null;

        await ExecuteAsync(async () =>
        {
            var invoiceNum = $"POS-{DateTime.Now:yyyyMMddHHmmss}";
            var req = new CreateSalesInvoiceRequest
            {
                InvoiceNumber = invoiceNum,
                BranchId = SelectedBranch.Id,
                WarehouseId = SelectedWarehouse.Id,
                CustomerId = SelectedCustomer?.Id,
                InvoiceDate = DateTime.UtcNow,
                Status = (SelectedPaymentMethod == PaymentMethod.Credit ? InvoiceStatus.Issued : InvoiceStatus.Paid),
                PaymentMethod = SelectedPaymentMethod,
                SubTotal = SubTotal,
                DiscountAmount = OrderDiscount,
                TaxAmount = TaxAmount,
                TotalAmount = GrandTotal,
                PaidAmount = (SelectedPaymentMethod == PaymentMethod.Credit ? PaidAmount : Math.Min(PaidAmount, GrandTotal)),
                Notes = OrderNotes,
                Items = CartItems.Select(c => new CreateSalesInvoiceItemRequest
                {
                    ProductId = c.ProductId,
                    ProductName = c.ProductName,
                    ProductBarCodeId = c.ProductBarCodeId,
                    Quantity = c.Quantity,
                    UnitPrice = c.UnitPrice,
                    DiscountAmount = c.DiscountAmount
                }).ToList()
            };

            var res = await _salesInvoiceApiService.CreateAsync(req);
            if (res.Success && res.Data != null)
            {
                var change = ChangeAmount;
                ClearCart();
                SuccessMessage = $"🎉 تم إتمام البيع بنجاح! رقم الفاتورة: {invoiceNum}" + (change > 0 ? $" (المتبقي للعميل: {change:N2} د.ل)" : string.Empty);
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل إتمام عملية البيع";
            }
        });
    }
}
