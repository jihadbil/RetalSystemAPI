using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

public interface IPurchaseInvoiceService
{
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

    Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default);
    Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default);
    Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
