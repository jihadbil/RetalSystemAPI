using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات أرصدة مخازن التخزين (StorgeStock) مع تضمين المستودع والباركود وبيانات المنتج وصوره.
/// </summary>
public class StorgeStockWithDetailsSpec : BaseSpecification<StorgeStock>
{
    /// <summary>
    /// جلب كافة أرصدة مخازن التخزين مع تضمين المستودع والباركود والمنتج والصور التابعة.
    /// </summary>
    public StorgeStockWithDetailsSpec()
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);

        // تضمين بيانات المنتج التابع للباركود
        AddInclude("ProductBarcode.Product");

        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>
    /// جلب أرصدة مخزن تخزين محدد بالمعرف مع التفاصيل الكاملة.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للمستودع</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);

        // تضمين بيانات المنتج التابع للباركود
        AddInclude("ProductBarcode.Product");

        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>
    /// جلب رصيد باركود محدد في مخزن تخزين معين.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للمستودع</param>
    /// <param name="productBarcodeId">المعرف الفريد لباركود المنتج</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId, Guid productBarcodeId)
        : base(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);

        // تضمين بيانات المنتج
        AddInclude("ProductBarcode.Product");

        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة مخزن تخزين مع الترقيم والبحث بالباركود أو اسم الصنف (أو المطابقة التامة).
    /// </summary>
    /// <param name="warehouseId">معرف المستودع</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
    /// <param name="isPaged">تطبيق التجزئة</param>
    public StorgeStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool exactBarcode = false, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.ProductBarcode != null && s.ProductBarcode.BarCode == searchTerm)
                        : ((s.ProductBarcode != null && s.ProductBarcode.BarCode != null && s.ProductBarcode.BarCode.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Title != null && s.ProductBarcode.Title.Contains(searchTerm)) ||
                           (s.ProductBarcode != null && s.ProductBarcode.Product != null && s.ProductBarcode.Product.Name != null && s.ProductBarcode.Product.Name.Contains(searchTerm))))))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);

        // تضمين بيانات المنتج التابع
        AddInclude("ProductBarcode.Product");

        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");

        // تطبيق التجزئة وترقيم الصفحات إن طُلب ذلك
        if (isPaged)
        {
            // حساب الإزاحة وحجم الصفحة
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}

/// <summary>
/// تخصيص عد أرصدة المخازن لتحديد إجمالي الصفحات في الاستعلامات المجزأة.
/// </summary>
public class StorgeStockCountSpec : BaseSpecification<StorgeStock>
{
    /// <summary>
    /// تهيئة مواصفة عد أرصدة مخزن تخزين محدد مع خيارات البحث بالباركود أو اسم الصنف.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
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
/// تخصيص استعلامات رصيد التخزين الأدنى (Low Storge Stock) لتنبيهات نقص الأرصدة ووصولها لحد إعادة الطلب.
/// </summary>
public class LowStorgeStockSpec : BaseSpecification<StorgeStock>
{
    /// <summary>
    /// تهيئة مواصفة تنبيه نقص مخزون التخزين وتضمين بيانات المنتج والصور.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع الاختياري</param>
    public LowStorgeStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات الباركود
        AddInclude(s => s.ProductBarcode);

        // تضمين بيانات المنتج التابع للباركود
        AddInclude("ProductBarcode.Product");

        // تضمين صور المنتج
        AddInclude("ProductBarcode.Product.ProductImages");
    }
}

/// <summary>
/// تخصيصات استعلامات مخزون صالات العرض (ShowroomStock) مع تضمين المستودع وبيانات المنتج وصوره.
/// </summary>
public class ShowroomStockWithDetailsSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>
    /// جلب كافة أرصدة صالات العرض مع المستودع والمنتج وصور المنتج.
    /// </summary>
    public ShowroomStockWithDetailsSpec()
    {
        // تضمين بيانات المستودع أو الصالة
        AddInclude(s => s.Warehouse);

        // تضمين بيانات المنتج
        AddInclude(s => s.Product);

        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب كافة أرصدة الأصناف لصالة عرض محددة بالمعرف.
    /// </summary>
    /// <param name="warehouseId">معرف صالة العرض</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات المنتج
        AddInclude(s => s.Product);

        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب رصيد صنف محدد في صالة عرض معينة.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة</param>
    /// <param name="productId">معرف المنتج</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, Guid productId)
        : base(s => s.WarehouseId == warehouseId && s.ProductId == productId)
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات المنتج
        AddInclude(s => s.Product);

        // تضمين صور المنتج
        AddInclude("Product.ProductImages");
    }

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة صالة العرض مع البحث باسم أو وصف الصنف (أو مطابقة باركوداته تماماً).
    /// </summary>
    /// <param name="warehouseId">معرف الصالة</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
    /// <param name="isPaged">تطبيق التجزئة</param>
    public ShowroomStockWithDetailsSpec(Guid warehouseId, int pageNumber, int pageSize, string? searchTerm = null, bool exactBarcode = false, bool isPaged = true)
        : base(s => s.WarehouseId == warehouseId &&
                   (string.IsNullOrWhiteSpace(searchTerm) ||
                    (exactBarcode
                        ? (s.Product != null && s.Product.ProductBarCodes.Any(b => b.BarCode == searchTerm))
                        : ((s.Product != null && s.Product.Name != null && s.Product.Name.Contains(searchTerm)) ||
                           (s.Product != null && s.Product.Description != null && s.Product.Description.Contains(searchTerm))))))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات المنتج
        AddInclude(s => s.Product);

        // تضمين صور المنتج
        AddInclude("Product.ProductImages");

        // تطبيق التجزئة وترقيم الصفحات إن طُلب ذلك
        if (isPaged)
        {
            // حساب الإزاحة وحجم الصفحة
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }
}

/// <summary>
/// تخصيص عد أرصدة صالة العرض لتحديد إجمالي الصفحات في الاستعلامات المجزأة.
/// </summary>
public class ShowroomStockCountSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>
    /// تهيئة مواصفة عد أرصدة صالة العرض مع البحث بالاسم أو الوصف أو الباركود.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
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
/// تخصيص استعلامات رصيد صالة العرض الأدنى (Low Showroom Stock) لتنبيهات نقص الكميات ووصولها لحد إعادة الطلب.
/// </summary>
public class LowShowroomStockSpec : BaseSpecification<ShowroomStock>
{
    /// <summary>
    /// تهيئة مواصفة تنبيه نقص مخزون صالة العرض وتضمين بيانات المستودع والمنتج.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة الاختياري</param>
    public LowShowroomStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        // تضمين بيانات المستودع
        AddInclude(s => s.Warehouse);

        // تضمين بيانات المنتج
        AddInclude(s => s.Product);
    }
}
