using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Controllers.Accounting;

/// <summary>تقرير قراءة فقط لتدقيق اتساق المستندات والدفاتر والأرصدة المالية.</summary>
[Authorize]
[Route("api/accounting/reconciliation")]
public sealed class FinancialReconciliationController : BaseApiController
{
    private readonly IFinancialReconciliationService _service;

    public FinancialReconciliationController(IFinancialReconciliationService service)
    {
        _service = service;
    }

    /// <summary>ينفذ تدقيقاً مالياً للمستأجر الحالي دون تعديل البيانات.</summary>
    [HttpGet]
    [HasPermission(Permissions.FinancialReconciliation.View)]
    [EnableRateLimiting("bulk-operations")]
    public async Task<IActionResult> Audit(
        [FromQuery] Guid? branchId = null,
        [FromQuery] decimal tolerance = 0.01m,
        [FromQuery] int maxIssues = 500,
        CancellationToken ct = default)
    {
        var result = await _service.AuditAsync(branchId, tolerance, maxIssues, ct);
        return ToActionResult(result);
    }
}
