using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Customers.Interfaces;

namespace RetalSystemAPI.Controllers.Customers;

/// <summary>
/// متحكم إدارة العملاء وحساباتهم وأرقام هواتفهم.
/// </summary>
[Authorize]
[Route("api/customers")]
public class CustomersController : BaseApiController
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// الحصول على جميع العملاء مع إمكانية الفلترة بنوع العميل وحالة النشاط والبحث النصي.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CustomerType? type = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _customerService.GetAllAsync(type, isActive, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالعملاء مع دعم الفلترة والبحث.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CustomerType? type = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _customerService.GetPagedAsync(pageNumber, pageSize, type, isActive, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل عميل محدد بالمعرف بما في ذلك جهات الاتصال وأرقام الهواتف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Customers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _customerService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة عميل جديد مع إمكانية إرفاق أرقام هواتفه الأولية.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var result = await _customerService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<CustomerResponseDto>.Ok(result.Data!, "تم إضافة العميل بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات عميل قائم.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var result = await _customerService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف/أرشفة عميل.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _customerService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة نشاط العميل (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _customerService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة رقم هاتف جديد لعميل.
    /// </summary>
    [HttpPost("{customerId:guid}/phones")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> AddPhone([FromRoute] Guid customerId, [FromBody] CustomerPhoneDto dto, CancellationToken ct)
    {
        var result = await _customerService.AddPhoneAsync(customerId, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف رقم هاتف لعميل.
    /// </summary>
    [HttpDelete("{customerId:guid}/phones/{phoneId:guid}")]
    [HasPermission(Permissions.Customers.Edit)]
    public async Task<IActionResult> DeletePhone([FromRoute] Guid customerId, [FromRoute] Guid phoneId, CancellationToken ct)
    {
        var result = await _customerService.DeletePhoneAsync(customerId, phoneId, ct);
        return ToActionResult(result);
    }
}
