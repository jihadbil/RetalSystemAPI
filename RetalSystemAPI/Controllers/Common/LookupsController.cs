using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Controllers.Common;

/// <summary>Bounded, tenant-filtered selections for workflows that do not grant access to full entity records.</summary>
[Authorize]
[Route("api/lookups")]
public sealed class LookupsController(AppDbContext db) : BaseApiController
{
    [HttpGet("adjustment-quantity")]
    [HasPermission(Permissions.StockAdjustments.Create)]
    [HasPermission(Permissions.StockAdjustments.Approve)]
    public async Task<IActionResult> AdjustmentQuantity([FromQuery] Guid warehouseId, [FromQuery] Guid productId,
        [FromQuery] Guid? barcodeId, CancellationToken ct)
    {
        var warehouse = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == warehouseId && w.IsActive, ct);
        if (warehouse is null) return ToActionResult(ServiceResult<QuantityOption>.Failure("المستودع غير متاح", ErrorCodes.WarehouseNotFound));
        if (!await db.Products.AnyAsync(p => p.Id == productId, ct))
            return ToActionResult(ServiceResult<QuantityOption>.Failure("الصنف غير متاح", ErrorCodes.ProductNotFound));
        if (warehouse.Type == WarehouseType.Storge)
        {
            if (!barcodeId.HasValue || !await db.ProductBarCodes.AnyAsync(b => b.Id == barcodeId && b.ProductId == productId, ct))
                return ToActionResult(ServiceResult<QuantityOption>.Failure("حدد باركوداً ينتمي للصنف", ErrorCodes.ValidationError));
            var quantity = await db.StorgeStocks.Where(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == barcodeId)
                .Select(s => (int?)s.Quantity).SingleOrDefaultAsync(ct) ?? 0;
            return ToActionResult(ServiceResult<QuantityOption>.Success(new(quantity)));
        }
        var showroomQuantity = await db.ShowroomStocks.Where(s => s.WarehouseId == warehouseId && s.ProductId == productId)
            .Select(s => (int?)s.Quantity).SingleOrDefaultAsync(ct) ?? 0;
        return ToActionResult(ServiceResult<QuantityOption>.Success(new(showroomQuantity)));
    }

    [HttpGet("pos-payment-accounts")]
    [HasPermission(Permissions.Pos.View)]
    public Task<IActionResult> PosPaymentAccounts([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Safes.Where(s => s.IsActive && s.Type == SafeType.Bank && (!query.BranchId.HasValue || s.BranchId == query.BranchId))
            .Select(s => new Option { Id = s.Id, Name = s.Name, BranchId = s.BranchId, Type = (int)s.Type }), query, ct);

    [HttpGet("cash-drawers")]
    [HasAnyPermission(Permissions.Safes.View, Permissions.Safes.Edit)]
    public Task<IActionResult> CashDrawers([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Safes.Where(s => s.IsActive && s.Type == SafeType.CashDrawer && (!query.BranchId.HasValue || s.BranchId == query.BranchId))
            .Select(s => new Option { Id = s.Id, Name = s.Name, BranchId = s.BranchId, Type = (int)s.Type }), query, ct);

    [HttpGet("pos-terminals")]
    [HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> PosTerminals([FromQuery] LookupQuery query, CancellationToken ct) =>
        ToActionResult(ServiceResult<PagedResult<TerminalOption>>.Success(await ReadPage(db.PosTerminals
            .Where(t => !query.BranchId.HasValue || t.BranchId == query.BranchId)
            .Select(t => new TerminalOption { Id = t.Id, Name = t.Name, Code = t.Code, BranchId = t.BranchId,
                BranchName = t.Branch.Name, DefaultCashDrawerId = t.DefaultCashDrawerId,
                DefaultCashDrawerName = t.DefaultCashDrawer.Name, IsActive = t.IsActive, LastSeenAt = t.LastSeenAt }), query, ct)));

    [HttpGet("category-directory")]
    [HasPermission(Permissions.Categories.View)]
    public async Task<IActionResult> CategoryDirectory([FromQuery] LookupQuery query, CancellationToken ct) =>
        ToActionResult(ServiceResult<PagedResult<CategoryOption>>.Success(await ReadPage(db.Categories.Select(c => new CategoryOption
        { Id = c.Id, Name = c.Name, IsActive = c.IsActive, ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
            ProductCount = db.Products.Count(p => p.CategoryId == c.Id), SubCategoriesCount = db.Categories.Count(child => child.ParentCategoryId == c.Id) }), query, ct)));

    [HttpGet("unit-directory")]
    [HasPermission(Permissions.Units.View)]
    public async Task<IActionResult> UnitDirectory([FromQuery] LookupQuery query, CancellationToken ct) =>
        ToActionResult(ServiceResult<PagedResult<UnitDirectoryOption>>.Success(await ReadPage(db.Units.Select(u => new UnitDirectoryOption
        { Id = u.Id, Name = u.Name, Description = u.Description, UnitPackage = u.UnitPackage }), query, ct)));

    [HttpGet("sales-return-invoices")]
    [HasPermission(Permissions.SalesReturns.Create)]
    public async Task<IActionResult> SalesReturnInvoices([FromQuery] LookupQuery query, CancellationToken ct)
    {
        var source = db.SalesInvoices.Where(i => (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.PartiallyPaid)
            && (!query.BranchId.HasValue || i.BranchId == query.BranchId))
            .Select(i => new ReturnSource { Id = i.Id, Name = i.InvoiceNumber, InvoiceNumber = i.InvoiceNumber,
                BranchId = i.BranchId, WarehouseId = i.WarehouseId, CustomerId = i.CustomerId,
                CustomerName = i.Customer != null ? i.Customer.Name : null, Status = (int)i.Status, TotalAmount = i.TotalAmount });
        var page = await ReadPage(source, query, ct);
        if (query.Id.HasValue && page.Items.Count == 1)
            page.Items[0].Items = await db.SalesInvoiceItems.AsNoTracking().Where(i => i.SalesInvoiceId == query.Id)
                .OrderBy(i => i.Id).Select(i => new ReturnSourceItem { ProductId = i.ProductId, ProductName = i.Product.Name,
                    ProductBarCodeId = i.ProductBarCodeId, Quantity = i.Quantity, UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount, LineTotal = i.LineTotal }).ToListAsync(ct);
        return ToActionResult(ServiceResult<PagedResult<ReturnSource>>.Success(page));
    }

    [HttpGet("purchase-return-invoices")]
    [HasPermission(Permissions.PurchaseReturns.Create)]
    public async Task<IActionResult> PurchaseReturnInvoices([FromQuery] LookupQuery query, CancellationToken ct)
    {
        var source = db.PurchaseInvoices.Where(i => (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.PartiallyPaid)
            && (!query.BranchId.HasValue || i.BranchId == query.BranchId))
            .Select(i => new ReturnSource { Id = i.Id, Name = i.InvoiceNumber, InvoiceNumber = i.InvoiceNumber,
                BranchId = i.BranchId, WarehouseId = i.WarehouseId, SupplierId = i.SupplierId,
                SupplierName = i.Supplier.Name, Status = (int)i.Status, TotalAmount = i.TotalAmount });
        var page = await ReadPage(source, query, ct);
        if (query.Id.HasValue && page.Items.Count == 1)
            page.Items[0].Items = await db.PurchaseInvoiceItems.AsNoTracking().Where(i => i.PurchaseInvoiceId == query.Id)
                .OrderBy(i => i.Id).Select(i => new ReturnSourceItem { ProductId = i.ProductId, ProductName = i.Product.Name,
                    ProductBarCodeId = i.ProductBarCodeId, Quantity = i.Quantity, UnitPrice = i.UnitPrice,
                    DiscountAmount = i.DiscountAmount, LineTotal = i.LineTotal,
                    Breakdowns = i.Breakdowns.OrderBy(b => b.Id).Select(b => new ReturnSourceBreakdown(b.ProductBarCodeId, b.Quantity, b.UnitPrice)).ToList() })
                .AsSplitQuery().ToListAsync(ct);
        return ToActionResult(ServiceResult<PagedResult<ReturnSource>>.Success(page));
    }

    [HttpGet("warehouses")]
    [HasAnyPermission(Permissions.Warehouses.View, Permissions.Stock.View, Permissions.StockTransfers.View,
        Permissions.StockAdjustments.View, Permissions.Pos.View, Permissions.SalesInvoices.Create,
        Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit, Permissions.PurchaseOrders.Create,
        Permissions.SalesReturns.Create, Permissions.PurchaseReturns.Create, Permissions.Products.Create,
        Permissions.Products.Edit, Permissions.StockTransfers.Create, Permissions.StockAdjustments.Create, Permissions.PurchaseOrders.Edit)]
    public Task<IActionResult> Warehouses([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Warehouses.Where(w => w.IsActive && (!query.BranchId.HasValue || w.BranchId == query.BranchId))
            .Select(w => new Option { Id = w.Id, Name = w.Name, BranchId = w.BranchId, Type = (int)w.Type }), query, ct);

    [HttpGet("branches")]
    [HasAnyPermission(Permissions.Branches.View, Permissions.Users.Create, Permissions.Users.Edit,
        Permissions.Warehouses.Create, Permissions.Warehouses.Edit, Permissions.Safes.Create, Permissions.Safes.Edit,
        Permissions.Expenses.Create, Permissions.Customers.Edit, Permissions.Suppliers.Edit,
        Permissions.Stock.View, Permissions.StockTransfers.Create, Permissions.StockAdjustments.Create,
        Permissions.SalesReturns.Create, Permissions.PurchaseReturns.Create, Permissions.Pos.View, Permissions.Pos.CreateSale,
        Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit, Permissions.SalesInvoices.Create,
        Permissions.PurchaseOrders.Create, Permissions.PurchaseOrders.Edit)]
    public Task<IActionResult> Branches([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Branches.Where(b => b.IsActive).Select(b => new Option { Id = b.Id, Name = b.Name }), query, ct);

    [HttpGet("suppliers")]
    [HasAnyPermission(Permissions.Suppliers.View, Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit,
        Permissions.PurchaseOrders.Create, Permissions.PurchaseOrders.Edit, Permissions.PurchaseReturns.Create)]
    public Task<IActionResult> Suppliers([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Suppliers.Where(s => s.IsActive).Select(s => new Option { Id = s.Id, Name = s.Name }), query, ct);

    [HttpGet("customers")]
    [HasAnyPermission(Permissions.Customers.View, Permissions.SalesInvoices.Create, Permissions.SalesReturns.Create,
        Permissions.Pos.View, Permissions.Pos.CreateSale)]
    public Task<IActionResult> Customers([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Customers.Where(c => c.IsActive).Select(c => new Option { Id = c.Id, Name = c.Name }), query, ct);

    [HttpGet("safes")]
    [HasAnyPermission(Permissions.Safes.View, Permissions.Safes.Transfer, Permissions.Safes.Transactions,
        Permissions.Expenses.Create, Permissions.Customers.Edit, Permissions.Suppliers.Edit,
        Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit, Permissions.SalesInvoices.Create,
        Permissions.SalesReturns.Create, Permissions.PurchaseReturns.Create)]
    public Task<IActionResult> Safes([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Safes.Where(s => s.IsActive && (!query.BranchId.HasValue || s.BranchId == query.BranchId))
            .Select(s => new Option { Id = s.Id, Name = s.Name, BranchId = s.BranchId, Type = (int)s.Type }), query, ct);

    [HttpGet("expense-categories")]
    [HasAnyPermission(Permissions.ExpenseCategories.View, Permissions.Expenses.View, Permissions.Expenses.Create)]
    public Task<IActionResult> ExpenseCategories([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.ExpenseCategories.Where(c => c.IsActive).Select(c => new Option { Id = c.Id, Name = c.Name }), query, ct);

    [HttpGet("categories")]
    [HasAnyPermission(Permissions.Categories.View, Permissions.Categories.Create, Permissions.Categories.Edit,
        Permissions.Products.View, Permissions.Products.Create, Permissions.Products.Edit)]
    public Task<IActionResult> Categories([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Categories.Where(c => c.IsActive).Select(c => new Option { Id = c.Id, Name = c.Name }), query, ct);

    [HttpGet("units")]
    [HasAnyPermission(Permissions.Units.View, Permissions.Products.Create, Permissions.Products.Edit)]
    public Task<IActionResult> Units([FromQuery] LookupQuery query, CancellationToken ct) =>
        Page(db.Units.Select(u => new Option { Id = u.Id, Name = u.Name }), query, ct);

    [HttpGet("products")]
    [HasAnyPermission(Permissions.Products.View, Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit,
        Permissions.PurchaseOrders.Create, Permissions.PurchaseOrders.Edit, Permissions.PurchaseReturns.Create,
        Permissions.SalesInvoices.Create, Permissions.SalesReturns.Create, Permissions.StockTransfers.Create,
        Permissions.StockAdjustments.Create, Permissions.Pos.View, Permissions.Pos.CreateSale)]
    public async Task<IActionResult> Products([FromQuery] LookupQuery query, CancellationToken ct)
    {
        var source = db.Products.AsNoTracking();
        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            source = source.Where(p => p.Name.Contains(search) || p.ProductBarCodes.Any(b => b.BarCode.Contains(search)));
        if (query.Id.HasValue) source = source.Where(p => p.Id == query.Id);
        if (!string.IsNullOrWhiteSpace(query.Barcode))
            source = source.Where(p => p.ProductBarCodes.Any(b => b.BarCode == query.Barcode));
        var cost = Can(Permissions.Products.View, Permissions.PurchaseInvoices.Create, Permissions.PurchaseInvoices.Edit,
            Permissions.PurchaseOrders.Create, Permissions.PurchaseOrders.Edit, Permissions.PurchaseReturns.Create)
            || Can(Permissions.StockAdjustments.Create) && Can(Permissions.StockAdjustments.Approve);
        var sale = cost || Can(Permissions.SalesInvoices.Create, Permissions.SalesReturns.Create, Permissions.Pos.View, Permissions.Pos.CreateSale);
        var count = await source.CountAsync(ct);
        var page = Math.Clamp(query.PageNumber, 1, 1000000);
        var size = Math.Clamp(query.PageSize, 1, 100);
        var selection = source.OrderBy(p => p.Name).ThenBy(p => p.Id).Skip((page - 1) * size).Take(size);
        var items = query.Id.HasValue || !string.IsNullOrWhiteSpace(query.Barcode)
            ? await selection.Select(ProjectProduct(cost, sale)).AsSplitQuery().ToListAsync(ct)
            : await selection.Select(p => new ProductOption { Id = p.Id, Name = p.Name, CategoryId = p.CategoryId,
                Code = p.ProductBarCodes.OrderBy(b => b.Id).Select(b => b.BarCode).FirstOrDefault() ?? "",
                CostPrice = cost ? p.CostPrice : 0, AveragePrice = cost ? p.AveragePrice : 0, SalePrice = sale ? p.SalePrice : 0 }).ToListAsync(ct);
        return ToActionResult(ServiceResult<PagedResult<ProductOption>>.Success(PagedResult<ProductOption>.Create(items, count, page, size)));
    }

    private bool Can(params string[] permissions) => User.IsInRole("SuperAdmin") || User.Claims.Any(c =>
        (string.Equals(c.Type, Permissions.ClaimType, StringComparison.OrdinalIgnoreCase) || string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase)) &&
        permissions.Contains(c.Value, StringComparer.OrdinalIgnoreCase));

    private async Task<IActionResult> Page(IQueryable<Option> source, LookupQuery query, CancellationToken ct)
        => ToActionResult(ServiceResult<PagedResult<Option>>.Success(await ReadPage(source, query, ct)));

    private static async Task<PagedResult<T>> ReadPage<T>(IQueryable<T> source, LookupQuery query, CancellationToken ct) where T : Option
    {
        source = source.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search)) source = source.Where(o => o.Name.Contains(query.Search.Trim()));
        if (query.Id.HasValue) source = source.Where(o => o.Id == query.Id);
        var page = Math.Clamp(query.PageNumber, 1, 1000000);
        var size = Math.Clamp(query.PageSize, 1, 100);
        var count = await source.CountAsync(ct);
        var items = await source.OrderBy(o => o.Name).ThenBy(o => o.Id).Skip((page - 1) * size).Take(size).ToListAsync(ct);
        return PagedResult<T>.Create(items, count, page, size);
    }

    private static Expression<Func<Product, ProductOption>> ProjectProduct(bool cost, bool sale) => p => new ProductOption
    {
        Id = p.Id, Name = p.Name, CategoryId = p.CategoryId,
        Code = p.ProductBarCodes.OrderBy(b => b.Id).Select(b => b.BarCode).FirstOrDefault() ?? "",
        CostPrice = cost ? p.CostPrice : 0, AveragePrice = cost ? p.AveragePrice : 0, SalePrice = sale ? p.SalePrice : 0,
        Units = p.ProductUnits.OrderBy(u => u.Id).Select(u => new UnitOption(u.UnitId, u.Unit!.Name, u.ConversionFactor, u.IsDefault)).ToList(),
        BarCodes = p.ProductBarCodes.OrderBy(b => b.Id).Select(b => new BarcodeOption(b.Id, b.BarCode, b.Title)).ToList()
    };

    public sealed class LookupQuery
    {
        public string? Search { get; set; }
        public Guid? Id { get; set; }
        public Guid? BranchId { get; set; }
        public string? Barcode { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
    public class Option
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public Guid? BranchId { get; set; }
        public int? Type { get; set; }
    }
    public sealed class ProductOption : Option
    {
        public Guid CategoryId { get; set; }
        public string Code { get; set; } = "";
        public decimal CostPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal SalePrice { get; set; }
        public List<UnitOption> Units { get; set; } = [];
        public List<BarcodeOption> BarCodes { get; set; } = [];
    }
    public sealed record UnitOption(Guid UnitId, string UnitName, int ConversionFactor, bool IsDefault);
    public sealed record BarcodeOption(Guid Id, string BarCode, string Title);
    public sealed record QuantityOption(int Quantity);
    public sealed class CategoryOption : Option
    {
        public Guid? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int ProductCount { get; set; }
        public int SubCategoriesCount { get; set; }
    }
    public sealed class TerminalOption : Option
    {
        public string Code { get; set; } = "";
        public string BranchName { get; set; } = "";
        public Guid DefaultCashDrawerId { get; set; }
        public string DefaultCashDrawerName { get; set; } = "";
        public DateTime? LastSeenAt { get; set; }
    }
    public sealed class UnitDirectoryOption : Option
    {
        public string? Description { get; set; }
        public int UnitPackage { get; set; }
    }
    public sealed class ReturnSource : Option
    {
        public string InvoiceNumber { get; set; } = "";
        public Guid WarehouseId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ReturnSourceItem> Items { get; set; } = [];
    }
    public sealed class ReturnSourceItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public Guid? ProductBarCodeId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
        public List<ReturnSourceBreakdown> Breakdowns { get; set; } = [];
    }
    public sealed record ReturnSourceBreakdown(Guid ProductBarCodeId, decimal Quantity, decimal UnitPrice);
}
