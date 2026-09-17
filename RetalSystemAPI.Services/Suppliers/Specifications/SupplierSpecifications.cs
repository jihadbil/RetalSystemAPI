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
    /// <summary>
    /// جلب كافة الموردين مع تضمين أرقام هواتفهم وترتيبهم تصاعدياً بالاسم.
    /// </summary>
    public SupplierWithDetailsSpec()
    {
        // تضمين هواتف المورد
        AddInclude(s => s.SupplierPhones);

        // الترتيب تصاعدياً باسم المورد
        ApplyOrderBy(s => s.Name);
    }

    /// <summary>
    /// جلب مورد محدد بالمعرف الفريد مع تضمين هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد</param>
    public SupplierWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        // تضمين هواتف المورد
        AddInclude(s => s.SupplierPhones);
    }

    /// <summary>
    /// البحث في الموردين بمطابقة الاسم أو العنوان أو أي رقم هاتف تابع.
    /// </summary>
    /// <param name="search">نص البحث المطلوب</param>
    public SupplierWithDetailsSpec(string? search)
        : base(s => string.IsNullOrWhiteSpace(search) || 
                    // فحص مطابقة اسم المورد
                    s.Name.Contains(search) || 
                    // فحص مطابقة العنوان
                    (s.Address != null && s.Address.Contains(search)) ||
                    // فحص مطابقة أي رقم هاتف تابع للمورد
                    s.SupplierPhones.Any(p => p.PhoneNumber.Contains(search)))
    {
        // تضمين هواتف المورد
        AddInclude(s => s.SupplierPhones);

        // الترتيب تصاعدياً بالاسم
        ApplyOrderBy(s => s.Name);
    }
}
