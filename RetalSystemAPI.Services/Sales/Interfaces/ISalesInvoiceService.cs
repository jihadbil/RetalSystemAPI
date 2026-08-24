using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة فواتير المبيعات وحركات البيع.
/// </summary>
public interface ISalesInvoiceService
{
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default);
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
    Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default);
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default);
    Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
