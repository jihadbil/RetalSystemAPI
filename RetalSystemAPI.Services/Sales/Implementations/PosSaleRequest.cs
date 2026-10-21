using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RetalSystemAPI.Models.DTOs.Sales;

namespace RetalSystemAPI.Services.Sales.Implementations;

internal static class PosSaleRequest
{
    // InvoiceNumber already has a tenant-scoped unique index. Preserve POS keys and
    // deterministically namespace older clients' keys instead of generating one per retry.
    internal static string InvoiceNumber(string clientNumber) =>
        clientNumber.StartsWith("POS-", StringComparison.Ordinal)
            ? clientNumber
            : $"POS-{Hash(clientNumber)[..32]}";

    internal static string Fingerprint(CreateSalesInvoiceDto dto) => Hash(JsonSerializer.Serialize(new
    {
        dto.BranchId, dto.WarehouseId, dto.CustomerId, dto.Status, dto.PaymentMethod,
        dto.PaymentSafeId, dto.CashSessionId, dto.Notes,
        Discount = Money(dto.DiscountAmount), Tax = Money(dto.TaxAmount), Paid = Money(dto.PaidAmount),
        Items = dto.Items.Select(i => new
        {
            i.ProductId, i.ProductBarCodeId, i.Quantity,
            Price = Money(i.UnitPrice), Discount = Money(i.DiscountAmount)
        })
    }));

    // Posting may resolve omitted barcodes. Keep the caller's request unchanged so
    // retry fingerprints describe the request, rather than server-generated defaults.
    internal static CreateSalesInvoiceDto ForPosting(CreateSalesInvoiceDto dto, string invoiceNumber) => new()
    {
        InvoiceNumber = invoiceNumber, InvoiceDate = dto.InvoiceDate,
        BranchId = dto.BranchId, WarehouseId = dto.WarehouseId, CustomerId = dto.CustomerId,
        Status = dto.Status, PaymentMethod = dto.PaymentMethod, SubTotal = dto.SubTotal,
        DiscountAmount = dto.DiscountAmount, TaxAmount = dto.TaxAmount, TotalAmount = dto.TotalAmount,
        PaidAmount = dto.PaidAmount, PaymentSafeId = dto.PaymentSafeId, CashSessionId = dto.CashSessionId,
        Notes = dto.Notes,
        Items = dto.Items.Select(i => new SalesInvoiceItemDto
        {
            ProductId = i.ProductId, ProductBarCodeId = i.ProductBarCodeId, Quantity = i.Quantity,
            UnitPrice = i.UnitPrice, UnitCost = i.UnitCost, DiscountAmount = i.DiscountAmount
        }).ToList()
    };

    private static string Money(decimal value) => value.ToString("G29", CultureInfo.InvariantCulture);
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
