using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات مخزون المخازن (StorgeStock).
/// </summary>
public class StorgeStockWithDetailsSpec : BaseSpecification<StorgeStock>
{
    public StorgeStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }

    public StorgeStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }

    public StorgeStockWithDetailsSpec(Guid warehouseId, Guid productBarcodeId)
        : base(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }

    public StorgeStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) ||
                    (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) ||
                    (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");

        if (isPaged)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}

/// <summary>
/// تخصيص عد أرصدة المخازن لتحديد إجمالي الصفحات.
/// </summary>
public class StorgeStockCountSpec : BaseSpecification<StorgeStock>
{
    public StorgeStockCountSpec(Guid warehouseId, string? searchTerm = null)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) ||
                    (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) ||
                    (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))
    {
    }
}

/// <summary>
/// تخصيص استعلامات رصيد التخزين الأدنى (Low Storge Stock).
/// </summary>
public class LowStorgeStockSpec : BaseSpecification<StorgeStock>
{
    public LowStorgeStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }
}

/// <summary>
/// تخصيصات استعلامات مخزون صالات العرض (ShowroomStock).
/// </summary>
public class ShowroomStockWithDetailsSpec : BaseSpecification<ShowroomStock>
{
    public ShowroomStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }

    public ShowroomStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }

    public ShowroomStockWithDetailsSpec(Guid warehouseId, Guid productId)
        : base(s => s.WarehouseId == warehouseId && s.ProductId == productId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }

    public ShowroomStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) ||
                    (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);

        if (isPaged)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}

/// <summary>
/// تخصيص عد أرصدة صالة العرض لتحديد إجمالي الصفحات.
/// </summary>
public class ShowroomStockCountSpec : BaseSpecification<ShowroomStock>
{
    public ShowroomStockCountSpec(Guid warehouseId, string? searchTerm = null)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) ||
                    (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))
    {
    }
}

/// <summary>
/// تخصيص استعلامات رصيد صالة العرض الأدنى (Low Showroom Stock).
/// </summary>
public class LowShowroomStockSpec : BaseSpecification<ShowroomStock>
{
    public LowShowroomStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }
}
