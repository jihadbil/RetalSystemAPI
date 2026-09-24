using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Services.Accounting.Interfaces;

namespace RetalSystemAPI.Controllers.Accounting;

[Authorize]
[Route("api/pos-terminals")]
public class PosTerminalsController : BaseApiController
{
    private readonly IPosCashService _service;
    public PosTerminalsController(IPosCashService service) => _service = service;

    [HttpGet, HasPermission(Permissions.Safes.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ToActionResult(await _service.GetTerminalsAsync(ct));

    [HttpPost, HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> Create([FromBody] CreatePosTerminalDto dto, CancellationToken ct) => ToActionResult(await _service.CreateTerminalAsync(dto, ct));

    [HttpPut("{id:guid}"), HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePosTerminalDto dto, CancellationToken ct) => ToActionResult(await _service.UpdateTerminalAsync(id, dto, ct));

    [HttpPost("{id:guid}/pairing-code"), HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> GeneratePairingCode(Guid id, CancellationToken ct) => ToActionResult(await _service.GeneratePairingCodeAsync(id, ct));
}

[Authorize]
[Route("api/pos-device")]
public class PosDeviceController : BaseApiController
{
    private const string DeviceCookie = "retal_pos_device";
    private readonly IPosCashService _service;
    public PosDeviceController(IPosCashService service) => _service = service;

    [AllowAnonymous, EnableRateLimiting("authentication")]
    [HttpPost("claim")]
    public async Task<IActionResult> Claim([FromBody] ClaimPosDeviceDto dto, CancellationToken ct)
    {
        var result = await _service.ClaimDeviceAsync(dto, ct);
        if (result.IsSuccess)
        {
            Response.Cookies.Append(DeviceCookie, result.Data!.DeviceToken, new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None,
                Expires = new DateTimeOffset(result.Data.ExpiresAt), IsEssential = true
            });
        }
        return ToActionResult(result);
    }

    [HttpGet("context")]
    public async Task<IActionResult> Context(CancellationToken ct) => ToActionResult(await _service.GetContextAsync(GetDeviceToken(), ct));

    [HttpPost("sessions/open"), HasPermission(Permissions.Pos.OpenSession)]
    public async Task<IActionResult> Open([FromBody] OpenCashSessionDto dto, CancellationToken ct) => ToActionResult(await _service.OpenSessionAsync(GetDeviceToken(), dto, ct));

    [HttpPost("sessions/close"), HasPermission(Permissions.Pos.CloseSession)]
    public async Task<IActionResult> Close([FromBody] CloseCashSessionDto dto, CancellationToken ct) => ToActionResult(await _service.CloseSessionAsync(GetDeviceToken(), dto, ct));

    private string? GetDeviceToken() =>
        Request.Headers.TryGetValue("X-POS-Device-Token", out var header) && !string.IsNullOrWhiteSpace(header)
            ? header.ToString()
            : Request.Cookies[DeviceCookie];
}

[Authorize]
[Route("api/cash-sessions")]
public class CashSessionsController : BaseApiController
{
    private readonly IPosCashService _service;
    public CashSessionsController(IPosCashService service) => _service = service;

    [HttpPost("{id:guid}/approve-difference"), HasPermission(Permissions.Safes.Edit)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveCashDifferenceDto dto, CancellationToken ct) =>
        ToActionResult(await _service.ApproveDifferenceAsync(id, dto, ct));
}
