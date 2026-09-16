using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Suppliers.Interfaces;

namespace RetalSystemAPI.Controllers.Suppliers;

/// <summary>
/// متحكم إدارة الموردين وأرقام هواتفهم.
/// </summary>
[Authorize]
[Route("api/suppliers")]
public class SuppliersController : BaseApiController
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// الحصول على قائمة بكل الموردين ملخصين.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _supplierService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية للموردين مع دعم البحث بالاسم أو الهواتف.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _supplierService.GetPagedAsync(pageNumber, pageSize, search, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل المورد وهواتفه بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _supplierService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة مورد جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Suppliers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto, CancellationToken ct)
    {
        var result = await _supplierService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<SupplierResponseDto>.Ok(result.Data!, "تم إضافة المورد بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات مورد موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSupplierDto dto, CancellationToken ct)
    {
        var result = await _supplierService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مورد.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Suppliers.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _supplierService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إضافة رقم هاتف جديد للمورد.
    /// </summary>
    [HttpPost("{supplierId:guid}/phones")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> AddPhone([FromRoute] Guid supplierId, [FromBody] SupplierPhoneDto dto, CancellationToken ct)
    {
        var result = await _supplierService.AddPhoneAsync(supplierId, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف رقم هاتف للمورد.
    /// </summary>
    [HttpDelete("{supplierId:guid}/phones/{phoneId:guid}")]
    [HasPermission(Permissions.Suppliers.Edit)]
    public async Task<IActionResult> DeletePhone([FromRoute] Guid supplierId, [FromRoute] Guid phoneId, CancellationToken ct)
    {
        var result = await _supplierService.DeletePhoneAsync(supplierId, phoneId, ct);
        return ToActionResult(result);
    }
}
