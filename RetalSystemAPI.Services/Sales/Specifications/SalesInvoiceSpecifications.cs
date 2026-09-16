using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Services.Sales.Specifications;

/// <summary>
/// مواصفات استعلامات فواتير المبيعات مع تضمين بيانات الفروع والمستودعات والعملاء والأصناف والباركودات.
/// </summary>
public class SalesInvoiceWithDetailsSpec : BaseSpecification<SalesInvoice>
{
    /// <summary>جلب كافة فواتير المبيعات مرتبة تنازلياً بتاريخ الفاتورة</summary>
    public SalesInvoiceWithDetailsSpec()
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.InvoiceDate);
    }

    /// <summary>جلب فاتورة مبيعات محددة بالمعرف مع تفاصيلها</summary>
    /// <param name="id">معرف الفاتورة</param>
    public SalesInvoiceWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>جلب فاتورة مبيعات برقم الفاتورة</summary>
    /// <param name="invoiceNumber">رقم الفاتورة</param>
    public SalesInvoiceWithDetailsSpec(string invoiceNumber) : base(s => s.InvoiceNumber == invoiceNumber)
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>فلترة فواتير المبيعات بالفرع والمستودع والعميل والحالة وطريقة الدفع والتاريخ والبحث</summary>
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

/// <summary>
/// مواصفة القوائم والترقيم لفواتير المبيعات — التنقلات المباشرة وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
public class SalesInvoiceListSpec : BaseSpecification<SalesInvoice>
{
    /// <summary>جلب كافة فواتير المبيعات للقوائم مرتبة تنازلياً بتاريخ الفاتورة</summary>
    public SalesInvoiceListSpec()
    {
        AddInclude(s => s.Branch);
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Customer!);
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.InvoiceDate);
    }

    /// <summary>فلترة قوائم فواتير المبيعات بالفرع والمستودع والعميل والحالة وطريقة الدفع والتاريخ والبحث</summary>
    public SalesInvoiceListSpec(
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
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.InvoiceDate);
    }
}
