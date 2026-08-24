using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Customers;

/// <summary>
/// أرقام هواتف العملاء وجهات الاتصال المرتبطة بهم.
/// </summary>
public class CustomerPhone : TenantBaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// رقم الهاتف.
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// اسم صاحب الهاتف (اختياري).
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// هل هو الرقم الرئيسي للاتصال.
    /// </summary>
    public bool IsDefault { get; set; } = false;
}
