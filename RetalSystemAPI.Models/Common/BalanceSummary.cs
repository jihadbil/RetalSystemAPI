namespace RetalSystemAPI.Models.Common;

/// <summary>Totals over the entire filtered customer/supplier set, before pagination.</summary>
public sealed class BalanceSummary
{
    public decimal TotalDebt { get; init; }
    public decimal TotalCredit { get; init; }
    public int WithDebt { get; init; }
}
