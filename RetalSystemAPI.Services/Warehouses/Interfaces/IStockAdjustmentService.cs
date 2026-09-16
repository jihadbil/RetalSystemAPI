using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة التسويات الجردية ومعالجة فوارق الجرد (زيادة/عجز/تلف/انتهاء صلاحية) وتعديل أرصدة المخازن الفعلية.
/// </summary>
public interface IStockAdjustmentService
{
    /// <summary>جلب تفاصيل تسوية جردية محددة بالمعرف مع كافة بنود التعديل وتفاصيل المستودع</summary>
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب تسوية جردية بواسطة رقم التسوية</summary>
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة التسويات الجردية مع إمكانية الفلترة بالمستودع وسبب التسوية والفترة الزمنية</summary>
    Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من التسويات الجردية مع الترقيم</summary>
    Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>إنشاء واعتماد تسوية جردية وتعديل كميات المخزون في المستودع المحدد تلقائياً وفق الفوارق</summary>
    Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default);

    /// <summary>حذف سجل تسوية جردية منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
