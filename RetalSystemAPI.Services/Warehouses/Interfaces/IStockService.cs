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

    /// <summary>جلب رصيد باركود/نكهة محددة في مخزن تخزين معين</summary>
    Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);

    /// <summary>جلب قائمة أرصدة كافة الباركودات في مخزن تخزين محدد</summary>
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من أرصدة مخزن تخزين مع الترقيم والبحث بالباركود أو اسم الصنف (مع خيار مطابقة الباركود تماماً)</summary>
    Task<ServiceResult<PagedResult<StorgeStockResponseDto>>> GetPagedStorgeStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default);

    /// <summary>تعيين أو تحديث رصيد الباركود وحد الطلب الأدنى في مخزن تخزين</summary>
    Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default);

    /// <summary>جلب تنبيهات الأرصدة التي وصلت أو قلت عن حد الطلب الأدنى لمخازن التخزين</summary>
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    // ── Showroom Stock (مخزون صالة العرض بالمنتج) ──────────

    /// <summary>جلب رصيد صنف محدد في صالة عرض معينة</summary>
    Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);

    /// <summary>جلب قائمة أرصدة كافة الأصناف في صالة عرض محددة</summary>
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من أرصدة صالة عرض مع الترقيم والبحث باسم الصنف (مع خيار مطابقة الباركود تماماً)</summary>
    Task<ServiceResult<PagedResult<ShowroomStockResponseDto>>> GetPagedShowroomStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default);

    /// <summary>تعيين أو تحديث رصيد الصنف وحد الطلب الأدنى في صالة العرض</summary>
    Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default);

    /// <summary>جلب تنبيهات الأصناف التي وصلت أو قلت عن حد الطلب الأدنى في صالات العرض</summary>
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);
}
