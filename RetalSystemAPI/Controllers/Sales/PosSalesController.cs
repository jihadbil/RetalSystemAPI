using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Sales.Interfaces;

namespace RetalSystemAPI.Controllers.Sales;

[Authorize]
[Route("api/pos-sales")]
public class PosSalesController(ISalesInvoiceService sales, IUnitOfWork unitOfWork) : BaseApiController
{
    [HttpPost]
    [HasPermission(Permissions.Pos.CreateSale)]
    public async Task<IActionResult> Create(CreateSalesInvoiceDto dto, CancellationToken ct) =>
        ToActionResult(await sales.CreatePosAsync(dto, ct));

    [HttpGet("payment-accounts")]
    [HasPermission(Permissions.Pos.View)]
    public async Task<IActionResult> GetPaymentAccounts([FromQuery] Guid branchId, CancellationToken ct)
    {
        var accounts = (await unitOfWork.Accounting.GetAllSafesAsync(ct))
            .Where(s => s.IsActive && s.Type == SafeType.Bank && s.BranchId == branchId)
            .Select(s => new PaymentAccount(s.Id, s.Name, s.BranchId)).ToList();
        return ToActionResult(ServiceResult<List<PaymentAccount>>.Success(accounts));
    }

    public sealed record PaymentAccount(Guid Id, string Name, Guid BranchId);
}
