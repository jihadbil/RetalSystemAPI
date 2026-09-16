using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات أرصدة مخازن التخزين (StorgeStock) مع تضمين المستودع والباركود وبيانات المنتج.
/// </summary>
public class StorgeStockWithDetailsSpec : BaseSpecification<StorgeStock>
{
    /// <summary>جلب كافة أرصدة مخازن التخزين مع المستودع والباركود</summary>
    public StorgeStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>جلب أرصدة مخزن تخزين محدد</summary>
    /// <param name="warehouseId">معرف المستودع</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>جلب رصيد باركود محدد في مخزن تخزين معين</summary>
    /// <param name="warehouseId">معرف المستودع</param>
    /// <param name="productBarcodeId">معرف الباركود</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId, Guid productBarcodeId)
        : base(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>جلب صفحة بيانات مجزأة من أرصدة مخزن تخزين مع إمكانية البحث بالباركود أو اسم الصنف — أو مطابقة الباركود تماماً</summary>
    public StorgeStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool exactBarcode = false, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.ProductBarcode != null && s.ProductBarcode.BarCode == searchTerm)
                        : ((s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
        AddInclude("ProductBarcode.Product.ProductImages");

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
    /// <summary>تهيئة مواصفة عد أرصدة مخزن تخزين محدد مع البحث — مع خيار مطابقة الباركود تماماً</summary>
    public StorgeStockCountSpec(Guid warehouseId, string? searchTerm = null, bool exactBarcode = false)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.ProductBarcode != null && s.ProductBarcode.BarCode == searchTerm)
                        : ((s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))))
    {
    }
}

/// <summary>
/// تخصيص استعلامات رصيد التخزين الأدنى (Low Storge Stock) لتنبيهات نقص الأرصدة.
/// </summary>
public class LowStorgeStockSpec : BaseSpecification<StorgeStock>
{
    /// <summary>تهيئة مواصفة تنبيه نقص مخزون التخزين</summary>
    /// <param name="warehouseId">معرف المستودع الاختياري</param>
    public LowStorgeStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
        AddInclude("ProductBarcode.Product.ProductImages");
    }
}

/// <summary>
/// تخصيصات استعلامات مخزون صالات العرض (ShowroomStock) مع تضمين المستودع وبيانات المنتج.
/// </summary>
public class ShowroomStockWithDetailsSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>جلب كافة أرصدة صالات العرض مع المستودع والمنتج</summary>
    public ShowroomStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
        AddInclude("Product.ProductImages");
    }

    /// <summary>جلب كافة أرصدة الأصناف لصالة عرض محددة</summary>
    /// <param name="warehouseId">معرف الصالة</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
        AddInclude("Product.ProductImages");
    }

    /// <summary>جلب رصيد صنف محدد في صالة عرض معينة</summary>
    /// <param name="warehouseId">معرف الصالة</param>
    /// <param name="productId">معرف الصنف</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, Guid productId)
        : base(s => s.WarehouseId == warehouseId && s.ProductId == productId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
        AddInclude("Product.ProductImages");
    }

    /// <summary>جلب صفحة بيانات مجزأة من أرصدة صالة العرض مع البحث باسم الصنف — أو مطابقة باركودات الصنف تماماً</summary>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool exactBarcode = false, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.Product != null && s.Product.ProductBarCodes.Any(b => b.BarCode == searchTerm))
                        : ((s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) ||
                           (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
        AddInclude("Product.ProductImages");

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
    /// <summary>تهيئة مواصفة عد أرصدة صالة العرض مع البحث — مع خيار مطابقة باركودات الصنف تماماً</summary>
    public ShowroomStockCountSpec(Guid warehouseId, string? searchTerm = null, bool exactBarcode = false)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.Product != null && s.Product.ProductBarCodes.Any(b => b.BarCode == searchTerm))
                        : ((s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) ||
                           (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))))
    {
    }
}

/// <summary>
/// تخصيص استعلامات رصيد صالة العرض الأدنى (Low Showroom Stock) لتنبيهات نقص الكميات.
/// </summary>
public class LowShowroomStockSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>تهيئة مواصفة تنبيه نقص مخزون صالة العرض</summary>
    /// <param name="warehouseId">معرف الصالة الاختياري</param>
    public LowShowroomStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }
}
