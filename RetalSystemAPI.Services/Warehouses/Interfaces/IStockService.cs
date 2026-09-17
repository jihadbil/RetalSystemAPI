using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة ومراقبة أرصدة المخزون لمستودعات التخزين (بالباركود/النكهة) وصالات العرض (بالصنف)، وتنبيهات انخفاض المخزون.
/// </summary>
public interface IStockService
{
    // ── Storge Stock (مخزون المخازن بالباركود) ───────────

    /// <summary>
    /// جلب رصيد باركود/نكهة محددة في مخزن تخزين معين.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للمستودع</param>
    /// <param name="productBarcodeId">المعرف الفريد للباركود</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات رصيد المخزن</returns>
    Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة أرصدة كافة الباركودات في مخزن تخزين محدد مع التغذية التلقائية للأصناف غير المسجلة.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للمستودع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة أرصدة الباركودات</returns>
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة مخزن تخزين مع الترقيم والبحث بالباركود أو اسم الصنف (مع خيار مطابقة الباركود تماماً).
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للمستودع</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على أرصدة الباركودات وإجمالي السجلات</returns>
    Task<ServiceResult<PagedResult<StorgeStockResponseDto>>> GetPagedStorgeStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default);

    /// <summary>
    /// تعيين أو تحديث رصيد الباركود وحد الطلب الأدنى في مخزن تخزين.
    /// </summary>
    /// <param name="dto">بيانات تعيين الرصيد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الرصيد المحدثة</returns>
    Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب تنبيهات الأرصدة التي وصلت أو قلت عن حد الطلب الأدنى لمخازن التخزين.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة تنبيهات النواقص</returns>
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    // ── Showroom Stock (مخزون صالة العرض بالمنتج) ──────────

    /// <summary>
    /// جلب رصيد صنف محدد في صالة عرض معينة.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للصالة</param>
    /// <param name="productId">المعرف الفريد للمنتج</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات رصيد الصالة</returns>
    Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة أرصدة كافة الأصناف في صالة عرض محددة مع التغذية التلقائية للمنتجات الجديدة.
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للصالة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة أرصدة أصناف الصالة</returns>
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أرصدة صالة عرض مع الترقيم والبحث باسم الصنف (مع خيار مطابقة الباركود تماماً).
    /// </summary>
    /// <param name="warehouseId">المعرف الفريد للصالة</param>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="searchTerm">نص البحث</param>
    /// <param name="exactBarcode">تفعيل المطابقة التامة للباركود</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على أرصدة الصالة وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<ShowroomStockResponseDto>>> GetPagedShowroomStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default);

    /// <summary>
    /// تعيين أو تحديث رصيد الصنف وحد الطلب الأدنى في صالة العرض.
    /// </summary>
    /// <param name="dto">بيانات تعيين الرصيد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الرصيد المحدثة</returns>
    Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب تنبيهات الأصناف التي وصلت أو قلت عن حد الطلب الأدنى في صالات العرض.
    /// </summary>
    /// <param name="warehouseId">معرف الصالة للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة تنبيهات النواقص بالصالات</returns>
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);
}
