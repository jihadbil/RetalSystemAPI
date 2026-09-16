using System;
using System.Linq;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Services.Customers.Specifications;

/// <summary>
/// تخصيصات استعلامات العملاء مع تفاصيل جهات الاتصال والهواتف والفلترة بالنوع والنشاط والبحث.
/// </summary>
public class CustomerWithDetailsSpec : BaseSpecification<Customer>
{
    /// <summary>جلب كافة العملاء مع هواتفهم مرتبين بالاسم</summary>
    public CustomerWithDetailsSpec()
    {
        AddInclude(c => c.CustomerPhones);
        ApplyOrderBy(c => c.Name);
    }

    /// <summary>جلب عميل محدد بالمعرف مع هواتفه</summary>
    /// <param name="id">معرف العميل</param>
    public CustomerWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        AddInclude(c => c.CustomerPhones);
    }

    /// <summary>البحث في العملاء بالاسم أو الكود أو الإيميل أو العنوان أو رقم الهاتف</summary>
    /// <param name="search">نص البحث</param>
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

    /// <summary>فلترة العملاء حسب النوع وحالة النشاط مع البحث النصي</summary>
    /// <param name="type">نوع العميل (عادي، جملة، VIP، مميز)</param>
    /// <param name="isActive">حالة النشاط</param>
    /// <param name="search">نص البحث</param>
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
/// مواصفة استعلام هواتف عميل محدد مرتبة بالأولوية للرقم الافتراضي.
/// </summary>
public class CustomerPhonesSpec : BaseSpecification<CustomerPhone>
{
    /// <summary>تهيئة مواصفة هواتف العميل</summary>
    /// <param name="customerId">معرف العميل</param>
    public CustomerPhonesSpec(Guid customerId) : base(p => p.CustomerId == customerId)
    {
        ApplyOrderByDescending(p => p.IsDefault);
    }
}
