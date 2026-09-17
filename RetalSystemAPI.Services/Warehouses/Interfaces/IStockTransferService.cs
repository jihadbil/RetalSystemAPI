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
    /// <summary>
    /// جلب تفاصيل أمر تحويل مخزني محدد بالمعرف مع كافة بنوده وتفاصيل المستودعات.
    /// </summary>
    /// <param name="id">المعرف الفريد لأمر التحويل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات أمر التحويل</returns>
    Task<ServiceResult<StockTransferResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب أمر تحويل مخزني بواسطة رقم أمر التحويل.
    /// </summary>
    /// <param name="transferNumber">رقم أمر التحويل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر التحويل</returns>
    Task<ServiceResult<StockTransferResponseDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة أوامر التحويل مع فلاتر المستودع المصدر، المستودع الهدف، الحالة، والفترة الزمنية.
    /// </summary>
    /// <param name="fromWarehouseId">معرف المستودع المصدر (اختياري)</param>
    /// <param name="toWarehouseId">معرف المستودع الوجهة (اختياري)</param>
    /// <param name="status">حالة أمر التحويل (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات أوامر التحويل</returns>
    Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من أوامر التحويل المخزني مع الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="fromWarehouseId">معرف المستودع المصدر (اختياري)</param>
    /// <param name="toWarehouseId">معرف المستودع الوجهة (اختياري)</param>
    /// <param name="status">حالة أمر التحويل (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات أوامر التحويل وإجمالي العدد</returns>
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

    /// <summary>
    /// إنشاء أمر تحويل مخزني جديد والتحقق من صحة المستودعات ورقم التحويل.
    /// </summary>
    /// <param name="dto">بيانات إنشاء أمر التحويل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر التحويل المنشأ</returns>
    Task<ServiceResult<StockTransferResponseDto>> CreateAsync(CreateStockTransferDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات وملاحظات أمر التحويل المخزني غير المكتمل.
    /// </summary>
    /// <param name="id">معرف أمر التحويل</param>
    /// <param name="dto">بيانات التحديث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر التحويل بعد التعديل</returns>
    Task<ServiceResult<StockTransferResponseDto>> UpdateAsync(Guid id, UpdateStockTransferDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث حالة أمر التحويل وإجراء المناقلة المخزنية الفعلية وتعديل الأرصدة في المصدر والوجهة عند الاكتمال.
    /// </summary>
    /// <param name="id">معرف أمر التحويل</param>
    /// <param name="status">الحالة الجديدة المراد الانتقال إليها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات أمر التحويل المحدثة</returns>
    Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default);

    /// <summary>
    /// حذف أمر تحويل مخزني منطقياً في حال عدم اكتماله وترحيله.
    /// </summary>
    /// <param name="id">معرف أمر التحويل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
