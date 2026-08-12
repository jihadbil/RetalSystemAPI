using System;
using System.Linq;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Services.Suppliers.Specifications;

/// <summary>
/// تخصيصات استعلامات الموردين.
/// </summary>
public class SupplierWithDetailsSpec : BaseSpecification<Supplier>
{
    public SupplierWithDetailsSpec()
    {
        AddInclude(s => s.SupplierPhones);
        ApplyOrderBy(s => s.Name);
    }

    public SupplierWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.SupplierPhones);
    }

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
