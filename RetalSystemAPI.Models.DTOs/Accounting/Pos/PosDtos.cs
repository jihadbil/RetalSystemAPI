using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Pos;

public class CreatePosTerminalDto
{
    [Required, MaxLength(50)] public string Code { get; set; } = null!;
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [Required] public Guid BranchId { get; set; }
    [Required] public Guid DefaultCashDrawerId { get; set; }
}

public class UpdatePosTerminalDto : CreatePosTerminalDto
{
    public bool IsActive { get; set; } = true;
}

public class PosTerminalResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid DefaultCashDrawerId { get; set; }
    public string DefaultCashDrawerName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? LastSeenAt { get; set; }
}

public class PairingCodeResponseDto
{
    public Guid TenantId { get; set; }
    public Guid TerminalId { get; set; }
    public string PairingCode { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}

public class ClaimPosDeviceDto
{
    [Required] public Guid TenantId { get; set; }
    [Required, MaxLength(20)] public string PairingCode { get; set; } = null!;
}

public class ClaimPosDeviceResponseDto
{
    public string DeviceToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public PosTerminalResponseDto Terminal { get; set; } = null!;
}

public class OpenCashSessionDto
{
    [Range(0, double.MaxValue)] public decimal OpeningFloat { get; set; }
}

public class CloseCashSessionDto
{
    [Range(0, double.MaxValue)] public decimal CountedClosingBalance { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class ApproveCashDifferenceDto
{
    [Required, MaxLength(500)] public string Notes { get; set; } = null!;
}

public class CashSessionResponseDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid PosTerminalId { get; set; }
    public string TerminalName { get; set; } = null!;
    public Guid CashDrawerId { get; set; }
    public string CashDrawerName { get; set; } = null!;
    public string CashierUserId { get; set; } = null!;
    public DateTime OpenedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal ExpectedClosingBalance { get; set; }
    public decimal? CountedClosingBalance { get; set; }
    public decimal? Difference { get; set; }
    public DateTime? ClosedAt { get; set; }
    public CashSessionStatus Status { get; set; }
    public string? ClosingNotes { get; set; }
    public string? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class PosContextDto
{
    public PosTerminalResponseDto Terminal { get; set; } = null!;
    public CashSessionResponseDto? CurrentSession { get; set; }
}
