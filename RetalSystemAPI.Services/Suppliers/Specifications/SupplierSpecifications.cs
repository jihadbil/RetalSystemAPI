using System;
using System.Linq;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Services.Suppliers.Specifications;

/// <summary>
/// تخصيصات استعلامات الموردين مع تضمين سجل الهواتف والترتيب والبحث.
/// </summary>
public class SupplierWithDetailsSpec : BaseSpecification<Supplier>
{
    /// <summary>جلب كافة الموردين مع هواتفهم مرتبين بالاسم</summary>
    public SupplierWithDetailsSpec()
    {
        AddInclude(s => s.SupplierPhones);
        ApplyOrderBy(s => s.Name);
    }

    /// <summary>جلب مورد محدد بالمعرف مع هواتفه</summary>
    /// <param name="id">معرف المورد</param>
    public SupplierWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.SupplierPhones);
    }

    /// <summary>البحث في الموردين بالاسم أو العنوان أو رقم الهاتف</summary>
    /// <param name="search">نص البحث</param>
    public SupplierWithDetailsSpec(string? search)
        : base(s => string.IsNullOrWhiteSpace(search) || 
                    s.Name.Contains(search) || 
                    (s.Address != null && s.Address.Contains(search)) ||
                    s.SupplierPhones.Any(p => p.PhoneNumber.Contains(search)))
    {
        AddInclude(s => s.SupplierPhones);
        ApplyOrderBy(s => s.Name);
    }
}
