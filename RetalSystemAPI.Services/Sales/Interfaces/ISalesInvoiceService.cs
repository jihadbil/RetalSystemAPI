using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة فواتير المبيعات ونقاط البيع (POS) وخصم الكميات من مخزون الصالة وترحيل القيود المالية.
/// </summary>
public interface ISalesInvoiceService
{
    /// <summary>
    /// إنشاء فاتورة مبيعات سريعة عبر نقطة البيع (POS) مع دعم وسائل الدفع المتعددة وتأمين البصمة ضد تكرار الإرسال.
    /// </summary>
    /// <param name="dto">بيانات فاتورة نقطة البيع ومدفوعاتها وبنودها</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>بيانات الفاتورة المنشأة أو المسترجعة في حال التكرار المتطابق</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> CreatePosAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل فاتورة مبيعات محددة بالمعرف مع بنودها وتفاصيل العميل والفرع وجهة الإصدار والمدفوعات.
    /// </summary>
    /// <param name="id">المعرف الفريد للفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات الفاتورة التفصيلية</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب فاتورة مبيعات بواسطة رقم الفاتورة المميز.
    /// </summary>
    /// <param name="invoiceNumber">رقم الفاتورة النصي</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة التفصيلية</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة فواتير المبيعات مع فلاتر الفرع، الصالة، العميل، الحالة، طريقة الدفع، والفترة الزمنية.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة (اختياري)</param>
    /// <param name="customerId">معرف العميل (اختياري)</param>
    /// <param name="status">حالة الفاتورة (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ بداية الفترة (اختياري)</param>
    /// <param name="toDate">تاريخ نهاية الفترة (اختياري)</param>
    /// <param name="search">نص البحث السريع (اختياري)</param>
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
    /// جلب صفحة بيانات مجزأة من فواتير المبيعات مع الترقيم والفلترة المتعددة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
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
    /// <param name="dto">بيانات إنشاء الفاتورة وبنودها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المنشأة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات وبنود فاتورة مبيعات غير مرحلة ومطابقة فوارق المخزون.
    /// </summary>
    /// <param name="id">معرف الفاتورة المراد تحديثها</param>
    /// <param name="dto">البيانات الجديدة للفاتورة والبنود</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المحدثة</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث حالة فاتورة المبيعات واسترجاع المخزون وعكس القيود عند الإلغاء.
    /// </summary>
    /// <param name="id">معرف الفاتورة</param>
    /// <param name="status">الحالة الجديدة المطلوب تطبيقها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة بعد تغيير حالتها</returns>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default);

    /// <summary>
    /// حذف فاتورة مبيعات منطقياً بعد التحقق من عدم وجود مرتجعات مرتبطة واسترجاع المخزون.
    /// </summary>
    /// <param name="id">معرف الفاتورة المراد حذفها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
