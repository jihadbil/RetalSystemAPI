using System;
using System.Linq;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Services.Customers.Specifications;

/// <summary>
/// تخصيصات استعلامات العملاء مع تفاصيل جهات الاتصال والهواتف.
/// </summary>
public class CustomerWithDetailsSpec : BaseSpecification<Customer>
{
    public CustomerWithDetailsSpec()
    {
        AddInclude(c => c.CustomerPhones);
        ApplyOrderBy(c => c.Name);
    }

    public CustomerWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        AddInclude(c => c.CustomerPhones);
    }

    public CustomerWithDetailsSpec(string? search)
        : base(c => string.IsNullOrWhiteSpace(search) ||
                    c.Name.Contains(search) ||
                    (c.Code != null && c.Code.Contains(search)) ||
                    (c.Email != null && c.Email.Contains(search)) ||
                    (c.Address != null && c.Address.Contains(search)) ||
                    c.CustomerPhones.Any(p => p.PhoneNumber.Contains(search)))
    {
        AddInclude(c => c.CustomerPhones);
        ApplyOrderBy(c => c.Name);
    }

    public CustomerWithDetailsSpec(CustomerType? type, bool? isActive, string? search = null)
        : base(c => (!type.HasValue || c.Type == type.Value) &&
                    (!isActive.HasValue || c.IsActive == isActive.Value) &&
                    (string.IsNullOrWhiteSpace(search) ||
                     c.Name.Contains(search) ||
                     (c.Code != null && c.Code.Contains(search)) ||
                     (c.Email != null && c.Email.Contains(search)) ||
                     (c.Address != null && c.Address.Contains(search)) ||
                     c.CustomerPhones.Any(p => p.PhoneNumber.Contains(search))))
    {
        AddInclude(c => c.CustomerPhones);
        ApplyOrderBy(c => c.Name);
    }
}

/// <summary>
/// تخصيص استعلامات هواتف عميل محدد.
/// </summary>
public class CustomerPhonesSpec : BaseSpecification<CustomerPhone>
{
    public CustomerPhonesSpec(Guid customerId) : base(p => p.CustomerId == customerId)
    {
        ApplyOrderByDescending(p => p.IsDefault);
    }
}
