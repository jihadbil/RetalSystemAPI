using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

/// <summary>
/// واجهة خدمة فواتير المشتريات وإدارة استلام البضائع والتغذية المخزنية متعددة النكهات والمحاسبة المالية.
/// </summary>
public interface IPurchaseInvoiceService
{
    /// <summary>
    /// جلب صفحة بيانات مجزأة من فواتير المشتريات مع دعم فلاتر متقدمة (المورد، الفرع، المستودع، الحالة، طريقة الدفع، النطاق الزمني والبحث).
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوب عرضها</param>
    /// <param name="pageSize">عدد الفواتير لكل صفحة</param>
    /// <param name="supplierId">معرف المورد (اختياري)</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="status">حالة الفاتورة (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ بداية النطاق الزمني (اختياري)</param>
    /// <param name="toDate">تاريخ نهاية النطاق الزمني (اختياري)</param>
    /// <param name="searchTerm">مصطلح البحث برقم الفاتورة أو اسم المورد (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>صفحة نتائج تحتوي على ملخصات الفواتير والعدد الإجمالي</returns>
    Task<ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل فاتورة مشتريات محددة مع بنودها وتفاصيل النكهات والباركودات المستلمة.
    /// </summary>
    /// <param name="id">معرف فاتورة المشتريات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>تفاصيل الفاتورة الكاملة</returns>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// إنشاء واعتماد فاتورة مشتريات جديدة وتحديث أرصدة المخازن آلياً وفق تفاصيل النكهات.
    /// </summary>
    /// <param name="dto">بيانات فاتورة المشتريات وبنودها وتفريعاتها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة المنشأة</returns>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث فاتورة مشتريات موجودة وإعادة مزامنة أرصدة المخازن للبنود الجديدة أو المحذوفة.
    /// </summary>
    /// <param name="id">معرف فاتورة المشتريات المراد تعديلها</param>
    /// <param name="dto">البيانات الجديدة للفاتورة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفاتورة بعد التحديث</returns>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default);

    /// <summary>
    /// إلغاء فاتورة مشتريات واسترجاع كميات المخزون المستلمة سابقاً إلى ما كانت عليه.
    /// </summary>
    /// <param name="id">معرف فاتورة المشتريات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الإلغاء</returns>
    Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// حذف فاتورة مشتريات منطقياً مع التحقق من عدم وجود مرتجعات مرتبطة بها وعكس المخزون.
    /// </summary>
    /// <param name="id">معرف فاتورة المشتريات المراد حذفها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
