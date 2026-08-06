using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// يمتل الوحدات في النظام (صندوق-دستة-قطعة) للماسعدة على ادخال البضاعة و حساب تكلفة القطعة بدل تغيير العبوة في كل عملية حساب
/// </summary>
public class Unit: BaseEntity
{

    /// <summary>
    /// اسم الوحدة  
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// وصف الوحدة
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// عدد القطع في الوحدة (مثلا عدد القطع في الصندوق او الدستة)
    /// </summary>
    public int UnitPackage { get; set; } = 1;

    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();
}
