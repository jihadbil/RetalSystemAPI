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

    /// <summary>جلب تفاصيل فاتورة مشتريات محددة مع بنودها وتفاصيل النكهات والباركودات المستلمة</summary>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>إنشاء واعتماد فاتورة مشتريات جديدة وتحديث أرصدة المخازن آلياً وفق تفاصيل النكهات</summary>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default);

    /// <summary>تحديث فاتورة مشتريات</summary>
    Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default);

    /// <summary>إلغاء فاتورة مشتريات واسترجاع كميات المخزون المستلمة سابقاً</summary>
    Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default);

    /// <summary>حذف فاتورة مشتريات منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
