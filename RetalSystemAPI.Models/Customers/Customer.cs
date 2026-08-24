using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Models.Customers;

/// <summary>
/// يمثل بيانات العملاء والزبائن الذين يتعامل معهم المحل.
/// </summary>
public class Customer : TenantBaseEntity
{
    /// <summary>
    /// اسم العميل.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// كود أو رقم العميل.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// البريد الإلكتروني.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// العنوان.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// تصنيف العميل (قطاعي / جملة / شركات).
    /// </summary>
    public CustomerType Type { get; set; } = CustomerType.Retail;

    /// <summary>
    /// الرصيد الافتتاحي للعميل عند بداية التعامل.
    /// </summary>
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>
    /// سقف الدين المسموح به (0 = غير محدود).
    /// </summary>
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>
    /// حالة نشاط العميل.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ICollection<CustomerPhone> CustomerPhones { get; set; } = new List<CustomerPhone>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
}

