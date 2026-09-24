namespace RetalSystemAPI.Desktop.Models.Pos;

public class PosTerminalInfo
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid BranchId { get; set; }
    public Guid DefaultCashDrawerId { get; set; }
    public string DefaultCashDrawerName { get; set; } = "";
}

public class CashSessionInfo
{
    public Guid Id { get; set; }
    public Guid CashDrawerId { get; set; }
    public string CashDrawerName { get; set; } = "";
    public decimal OpeningFloat { get; set; }
    public decimal ExpectedClosingBalance { get; set; }
    public decimal? CountedClosingBalance { get; set; }
    public decimal? Difference { get; set; }
    public int Status { get; set; }
}

public class PosDeviceContextInfo
{
    public PosTerminalInfo Terminal { get; set; } = null!;
    public CashSessionInfo? CurrentSession { get; set; }
}

public class ClaimPosDeviceResponse
{
    public string DeviceToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public PosTerminalInfo Terminal { get; set; } = null!;
}
