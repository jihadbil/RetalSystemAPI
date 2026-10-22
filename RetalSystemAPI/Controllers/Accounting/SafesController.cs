using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Accounting.Safes;
using RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Controllers.Accounting;

/// <summary>
/// متحكم إدارة الخزائن المالية والحسابات المصرفية وأدراج الكاشير وحركاتها والتحويلات المالية.
/// </summary>
[Authorize]
[Route("api/safes")]
public class SafesController : BaseApiController
{
    private readonly ISafeService _safeService;

    public SafesController(ISafeService safeService)
    {
        _safeService = safeService;
    }

    /// <summary>
    /// استرجاع كافة الخزائن والحسابات المصرفية المسجلة.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId = null,
        [FromQuery] RetalSystemAPI.Models.Enums.SafeType? type = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await _safeService.GetAllAsync(branchId, type, isActive, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع صفحة بيانات مجزأة للخزائن مع إمكانية الفلترة بالفرع وحالة النشاط والبحث النصي.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] Guid? branchId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RetalSystemAPI.Models.Enums.SafeType? type = null,
        CancellationToken ct = default)
    {
        var result = await _safeService.GetPagedAsync(branchId, isActive, search, pageNumber, pageSize, type, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل خزينة أو حساب مصرفي محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _safeService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء خزينة نقدية أو حساب مصرفي أو درج كاشير جديد مع تحديد الرصيد الافتتاحي.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Safes.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSafeDto dto, CancellationToken ct = default)
    {
        var result = await _safeService.CreateAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث بيانات خزينة أو حساب مصرفي موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateSafeDto dto, CancellationToken ct = default)
    {
        var result = await _safeService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> ToggleActive([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _safeService.ToggleActiveAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف ناعم لخزينة بشرط عدم وجود أي حركات مالية مسجلة عليها.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Safes.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _safeService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل حركة نقدية يدوية في الخزينة (إيداع أو سحب) مع تحديث رصيدها ذرياً.
    /// </summary>
    [HttpPost("transactions")]
    [HasPermission(Permissions.Safes.Transactions)]
    public async Task<IActionResult> AddTransaction([FromBody] CreateSafeTransactionDto dto, CancellationToken ct = default)
    {
        var result = await _safeService.AddTransactionAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحويل أموال بين خزينتين أو حسابين مصرفيين ضمن معاملة ذرية مؤمنة.
    /// </summary>
    [HttpPost("transfer")]
    [HasPermission(Permissions.Safes.Transfer)]
    public async Task<IActionResult> TransferFunds([FromBody] TransferFundsDto dto, CancellationToken ct = default)
    {
        var result = await _safeService.TransferFundsAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استعلام حركات الخزائن مفلترة بالتاريخ والنوع والخزينة مع الترقيم.
    /// </summary>
    [HttpGet("transactions")]
    [HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> GetTransactions([FromQuery] SafeTransactionFilterDto filter, CancellationToken ct = default)
    {
        var result = await _safeService.GetTransactionsAsync(filter, ct);
        return ToActionResult(result);
    }
}
