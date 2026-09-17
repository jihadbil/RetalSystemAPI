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
    /// <summary>
    /// جلب كافة العملاء مع هواتفهم مرتبين تصاعدياً بالاسم.
    /// </summary>
    public CustomerWithDetailsSpec()
    {
        // تضمين هواتف العميل
        AddInclude(c => c.CustomerPhones);

        // تطبيق الترتيب التصاعدي بحسب اسم العميل
        ApplyOrderBy(c => c.Name);
    }

    /// <summary>
    /// جلب عميل محدد بواسطة معرفه الفريد مع هواتفه التابعة.
    /// </summary>
    /// <param name="id">معرف العميل</param>
    public CustomerWithDetailsSpec(Guid id) : base(c => c.Id == id)
    {
        // تضمين أرقام هواتف العميل
        AddInclude(c => c.CustomerPhones);
    }

    /// <summary>
    /// البحث في العملاء بواسطة نص البحث (مطابقة الاسم، الكود، البريد، العنوان، أو أي رقم هاتف).
    /// </summary>
    /// <param name="search">نص البحث المطلوب</param>
    public CustomerWithDetailsSpec(string? search)
        : base(c => string.IsNullOrWhiteSpace(search) ||
                    // فحص مطابقة الاسم
                    c.Name.Contains(search) ||
                    // فحص مطابقة الكود إن وجد
                    (c.Code != null && c.Code.Contains(search)) ||
                    // فحص مطابقة البريد الإلكتروني إن وجد
                    (c.Email != null && c.Email.Contains(search)) ||
                    // فحص مطابقة العنوان إن وجد
                    (c.Address != null && c.Address.Contains(search)) ||
                    // فحص مطابقة أي رقم من أرقام الهواتف التابعة
                    c.CustomerPhones.Any(p => p.PhoneNumber.Contains(search)))
    {
        // تضمين أرقام الهواتف
        AddInclude(c => c.CustomerPhones);

        // الترتيب تصاعدياً بالاسم
        ApplyOrderBy(c => c.Name);
    }

    /// <summary>
    /// فلترة العملاء المتقدمة حسب النوع، حالة النشاط، مع نص البحث.
    /// </summary>
    /// <param name="type">نوع العميل (عادي، جملة، VIP، مميز)</param>
    /// <param name="isActive">حالة النشاط (نشط / غير نشط)</param>
    /// <param name="search">نص البحث الاختياري</param>
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
        // تضمين سجل الهواتف
        AddInclude(c => c.CustomerPhones);

        // الترتيب تصاعدياً بالاسم
        ApplyOrderBy(c => c.Name);
    }
}

/// <summary>
/// مواصفة استعلام أرقام هواتف عميل محدد مرتبة بالأولوية للرقم الافتراضي أولاً.
/// </summary>
public class CustomerPhonesSpec : BaseSpecification<CustomerPhone>
{
    /// <summary>
    /// تهيئة مواصفة هواتف العميل بالمعرف وتطبيق الترتيب التنازلي للرقم الافتراضي.
    /// </summary>
    /// <param name="customerId">المعرف الفريد للعميل</param>
    public CustomerPhonesSpec(Guid customerId) : base(p => p.CustomerId == customerId)
    {
        // إعطاء الأولوية في الترتيب للرقم الافتراضي
        ApplyOrderByDescending(p => p.IsDefault);
    }
}
