using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Customers.Interfaces;
using RetalSystemAPI.Services.Customers.Specifications;

namespace RetalSystemAPI.Services.Customers.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة العملاء وحساباتهم وأرقام هواتفهم.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(id), ct);
        if (customer is null)
        {
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        var dto = _mapper.Map<CustomerResponseDto>(customer);
        return ServiceResult<CustomerResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var spec = new CustomerWithDetailsSpec(type, isActive, search);
        var customers = await _unitOfWork.Customers.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<CustomerSummaryDto>>(customers);

        return ServiceResult<IReadOnlyList<CustomerSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default)
    {
        var spec = new CustomerWithDetailsSpec(type, isActive, search);
        var (items, totalCount) = await _unitOfWork.Customers.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<CustomerSummaryDto>>(items);
        var pagedResult = PagedResult<CustomerSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<CustomerSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            bool codeExists = await _unitOfWork.Customers.ExistsAsync(c => c.Code == dto.Code, ct);
            if (codeExists)
            {
                return ServiceResult<CustomerResponseDto>.Failure("كود العميل مستخدم بالفعل", ErrorCodes.CustomerCodeExists);
            }
        }

        var customer = _mapper.Map<Customer>(dto);
        if (dto.Phones != null && dto.Phones.Any())
        {
            customer.CustomerPhones = dto.Phones.Select(p => new CustomerPhone
            {
                PhoneNumber = p.PhoneNumber,
                ContactName = p.ContactName,
                IsDefault = p.IsDefault
            }).ToList();
        }

        await _unitOfWork.Customers.AddAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(customer.Id), ct) ?? customer;
        var responseDto = _mapper.Map<CustomerResponseDto>(created);

        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(id), ct);
        if (customer is null)
        {
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        if (!string.IsNullOrWhiteSpace(dto.Code) && dto.Code != customer.Code)
        {
            bool codeExists = await _unitOfWork.Customers.ExistsAsync(c => c.Code == dto.Code && c.Id != id, ct);
            if (codeExists)
            {
                return ServiceResult<CustomerResponseDto>.Failure("كود العميل مستخدم بالفعل لعميل آخر", ErrorCodes.CustomerCodeExists);
            }
        }

        customer.Name = dto.Name;
        customer.Code = dto.Code;
        customer.Email = dto.Email;
        customer.Address = dto.Address;
        customer.Type = dto.Type;
        customer.CreditLimit = dto.CreditLimit;
        customer.IsActive = dto.IsActive;

        if (dto.Phones != null)
        {
            if (customer.CustomerPhones != null && customer.CustomerPhones.Any())
            {
                foreach (var phone in customer.CustomerPhones.ToList())
                {
                    _unitOfWork.CustomerPhones.HardDelete(phone);
                }
            }

            customer.CustomerPhones = dto.Phones.Select(p => new CustomerPhone
            {
                CustomerId = id,
                PhoneNumber = p.PhoneNumber,
                ContactName = p.ContactName,
                IsDefault = p.IsDefault
            }).ToList();
        }

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(id), ct) ?? customer;
        var responseDto = _mapper.Map<CustomerResponseDto>(updated);

        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id, ct);
        if (customer is null)
        {
            return ServiceResult.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        _unitOfWork.Customers.SoftDelete(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<CustomerResponseDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(id), ct);
        if (customer is null)
        {
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        customer.IsActive = !customer.IsActive;
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<CustomerResponseDto>(customer);
        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<CustomerResponseDto>> AddPhoneAsync(Guid customerId, CustomerPhoneDto dto, CancellationToken ct = default)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(customerId), ct);
        if (customer is null)
        {
            return ServiceResult<CustomerResponseDto>.Failure("العميل غير موجود", ErrorCodes.CustomerNotFound);
        }

        if (dto.IsDefault && customer.CustomerPhones.Any(p => p.IsDefault))
        {
            foreach (var p in customer.CustomerPhones.Where(p => p.IsDefault))
            {
                p.IsDefault = false;
                _unitOfWork.CustomerPhones.Update(p);
            }
        }

        var phone = new CustomerPhone
        {
            CustomerId = customerId,
            PhoneNumber = dto.PhoneNumber,
            ContactName = dto.ContactName,
            IsDefault = dto.IsDefault
        };

        await _unitOfWork.CustomerPhones.AddAsync(phone, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.Customers.FirstOrDefaultAsync(new CustomerWithDetailsSpec(customerId), ct) ?? customer;
        var responseDto = _mapper.Map<CustomerResponseDto>(updated);

        return ServiceResult<CustomerResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default)
    {
        var phone = await _unitOfWork.CustomerPhones.GetByIdAsync(phoneId, ct);
        if (phone is null || phone.CustomerId != customerId)
        {
            return ServiceResult.Failure("رقم الهاتف غير موجود أو لا ينتمي لهذا العميل", ErrorCodes.NotFound);
        }

        _unitOfWork.CustomerPhones.SoftDelete(phone);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
