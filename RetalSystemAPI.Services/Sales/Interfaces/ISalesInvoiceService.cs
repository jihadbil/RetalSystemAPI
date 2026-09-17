using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة فواتير المبيعات ونقاط البيع وخصم الكميات من مخزون الصالة وحساب الضرائب والخصومات.
/// </summary>
public interface ISalesInvoiceService
{
    /// <summary>
    /// جلب تفاصيل فاتورة مبيعات محددة بالمعرف مع بنودها وتفاصيل العميل والفرع والصالة.
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات الفاتورة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب فاتورة مبيعات بواسطة رقم الفاتورة.
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة فواتير المبيعات مع فلاتر الفرع، الصالة، العميل، الحالة، طريقة الدفع، والفترة الزمنية.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة (اختياري)</param>
    /// <param name="customerId">معرف العميل (اختياري)</param>
    /// <param name="status">حالة الفاتورة (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات فواتير المبيعات</returns>
    Task<ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من فواتير المبيعات مع الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة (اختياري)</param>
    /// <param name="customerId">معرف العميل (اختياري)</param>
    /// <param name="status">حالة الفاتورة (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات الفواتير وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء واعتماد فاتورة مبيعات جديدة والتحقق من توفر الرصيد الكافي في صالة العرض أو المستودع وخصمه آلياً.
    /// </summary>
    /// <param name="dto">بيانات إنشاء الفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المنشأة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات فاتورة مبيعات غير مرحلة ومطابقة فوارق المخزون.
    /// </summary>
    /// <param name="id">معرف الفاتورة</param>
    /// <param name="dto">بيانات التحديث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المحدثة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث حالة فاتورة المبيعات واسترجاع المخزون عند الإلغاء.
    /// </summary>
    /// <param name="id">معرف الفاتورة</param>
    /// <param name="status">الحالة الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المحدثة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default);

    /// <summary>
    /// حذف فاتورة مبيعات منطقياً بعد التحقق من عدم وجود مرتجعات مرتبطة واسترجاع المخزون.
    /// </summary>
    /// <param name="id">معرف الفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
