using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Accounting.Expenses;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Controllers.Accounting;

/// <summary>
/// متحكم إدارة سندات المصروفات وسجلات الإنفاق التشغيلي في الفروع.
/// </summary>
[Authorize]
[Route("api/expenses")]
public class ExpensesController : BaseApiController
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    /// <summary>
    /// استرجاع صفحة بيانات مجزأة لسندات المصروفات مع الفلترة المتقدمة (بالفرع، التصنيف، الخزينة، طريقة الدفع، والتواريخ).
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Expenses.View)]
    public async Task<IActionResult> GetPaged([FromQuery] ExpenseFilterDto filter, CancellationToken ct = default)
    {
        var result = await _expenseService.GetPagedAsync(filter, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع تفاصيل سند مصروف محدد بالمعرف شاملاً بيانات الفرع والتصنيف والخزينة.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Expenses.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _expenseService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل سند مصروف تشغيلي جديد وخصمه آلياً من الخزينة المحددة.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Expenses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto, CancellationToken ct = default)
    {
        var result = await _expenseService.CreateAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل البيانات الوصفية لسند مصروف قائم (التصنيف، المستفيد، البيان، الملاحظات).
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Expenses.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateExpenseDto dto, CancellationToken ct = default)
    {
        var result = await _expenseService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إلغاء وحذف سند مصروف ناعماً مع استرداد قيمته للخزينة المسحوب منها آلياً بحركة عكسية.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Expenses.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await _expenseService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [HasPermission(Permissions.Expenses.Delete)]
    public async Task<IActionResult> Cancel([FromRoute] Guid id, [FromBody] CancelExpenseDto dto, CancellationToken ct = default)
    {
        var result = await _expenseService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }
}
