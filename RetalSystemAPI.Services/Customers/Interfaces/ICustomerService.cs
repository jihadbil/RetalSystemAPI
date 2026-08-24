using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Customers.Interfaces;

/// <summary>
/// واجهة خدمة إدارة العملاء وحساباتهم وأرقام هواتفهم.
/// </summary>
public interface ICustomerService
{
    Task<ServiceResult<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);
    Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<ServiceResult<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<CustomerResponseDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<CustomerResponseDto>> AddPhoneAsync(Guid customerId, CustomerPhoneDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default);
}
