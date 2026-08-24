using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Sales.Specifications;

/// <summary>
/// تخصيصات استعلامات فواتير المبيعات مع تفاصيل الفروع والمستودعات والعملاء والبنود.
/// </summary>
public class SalesInvoiceWithDetailsSpec : BaseSpecification<SalesInvoice>
{
    public SalesInvoiceWithDetailsSpec()
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.InvoiceDate);
    }

    public SalesInvoiceWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public SalesInvoiceWithDetailsSpec(string invoiceNumber) : base(s => s.InvoiceNumber == invoiceNumber)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public SalesInvoiceWithDetailsSpec(
        Guid? branchId,
        Guid? warehouseId,
        Guid? customerId,
        InvoiceStatus? status,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!branchId.HasValue || s.BranchId == branchId.Value) &&
                    (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value) &&
                    (!customerId.HasValue || s.CustomerId == customerId.Value) &&
                    (!status.HasValue || s.Status == status.Value) &&
                    (!paymentMethod.HasValue || s.PaymentMethod == paymentMethod.Value) &&
                    (!fromDate.HasValue || s.InvoiceDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.InvoiceDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     s.InvoiceNumber.Contains(search) ||
                     (s.Customer != null && s.Customer.Name.Contains(search))))
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.InvoiceDate);
    }
}
