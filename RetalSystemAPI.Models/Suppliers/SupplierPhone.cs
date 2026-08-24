using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;



namespace RetalSystemAPI.Models.Suppliers;
/// <summary>
/// يمتل ارقام الهواتف الخصة بالمورد و اسماء اصحابها
/// </summary>
public class SupplierPhone : TenantBaseEntity
{
    /// <summary>
    /// معرف المورد
    /// </summary>
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// رقم الهاتف
    /// </summary>
    public string PhoneNumber { get; set; } = null!;
    /// <summary>
    /// اسم صاحب الهاتف
    /// </summary>
    public string? Name { get; set; }




}
