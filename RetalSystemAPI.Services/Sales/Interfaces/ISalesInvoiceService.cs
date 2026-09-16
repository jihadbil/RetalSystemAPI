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
    /// <summary>جلب تفاصيل فاتورة مبيعات محددة بالمعرف مع بنودها وتفاصيل العميل والفرع والصالة</summary>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب فاتورة مبيعات بواسطة رقم الفاتورة</summary>
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة فواتير المبيعات مع فلاتر الفرع، الصالة، العميل، الحالة، طريقة الدفع، والفترة الزمنية</summary>
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

    /// <summary>جلب صفحة بيانات مجزأة من فواتير المبيعات مع الترقيم</summary>
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

    /// <summary>إنشاء واعتماد فاتورة مبيعات جديدة والتحقق من توفر الرصيد الكافي في صالة العرض وخصمه آلياً</summary>
    Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات فاتورة مبيعات</summary>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default);

    /// <summary>تحديث حالة فاتورة المبيعات</summary>
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default);

    /// <summary>حذف فاتورة مبيعات منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
