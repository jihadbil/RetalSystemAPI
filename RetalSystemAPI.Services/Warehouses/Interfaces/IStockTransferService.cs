using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة عمليات التحويل المخزني ونقل البضائع بين المستودعات والصالات وتحديث الأرصدة.
/// </summary>
public interface IStockTransferService
{
    /// <summary>جلب تفاصيل أمر تحويل مخزني محدد بالمعرف مع كافة بنوده وتفاصيل المستودعات</summary>
    Task<ServiceResult<StockTransferResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب أمر تحويل مخزني برقم أمر التحويل</summary>
    Task<ServiceResult<StockTransferResponseDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة أوامر التحويل مع فلاتر المستودع المصدر، المستودع الهدف، الحالة، والفترة الزمنية</summary>
    Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من أوامر التحويل المخزني مع الترقيم</summary>
    Task<ServiceResult<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>إنشاء أمر تحويل مخزني جديد والتحقق من كفاية الأرصدة في المستودع المصدر</summary>
    Task<ServiceResult<StockTransferResponseDto>> CreateAsync(CreateStockTransferDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات أمر التحويل المخزني</summary>
    Task<ServiceResult<StockTransferResponseDto>> UpdateAsync(Guid id, UpdateStockTransferDto dto, CancellationToken ct = default);

    /// <summary>تحديث حالة أمر التحويل وإجراء المناقلة المخزنية وتعديل الأرصدة عند الاكتمال أو الإلغاء</summary>
    Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default);

    /// <summary>حذف أمر تحويل مخزني منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
