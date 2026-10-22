namespace RetalSystemAPI.Models.DTOs.Sales;

public sealed class InvoiceIssuerDto
{
    public required string Name { get; init; }
    public string? TaxNumber { get; init; }
    public string? Address { get; init; }
    public string? Phone { get; init; }
}
