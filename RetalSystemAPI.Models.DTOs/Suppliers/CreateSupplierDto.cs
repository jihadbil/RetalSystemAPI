using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات إنشاء مورد جديد (Create Supplier DTO).
/// </summary>
public class CreateSupplierDto
{
    /// <summary>اسم الشركة الموردة التجارية أو المؤسسة</summary>
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    /// <summary>البريد الإلكتروني للتواصل مع المورد</summary>
    [MaxLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>الرقم الضريبي للمورد</summary>
    [MaxLength(100)]
    public string? TaxNumber { get; set; }

    /// <summary>ملاحظات إضافية حول المورد</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>حالة نشاط حساب المورد في النظام</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>اسم المورد أو جهة الاتصال الرئيسية</summary>
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(200, ErrorMessage = "اسم المورد يجب أن لا يتجاوز 200 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>عنوان مقر المورد</summary>
    [MaxLength(500, ErrorMessage = "العنوان يجب أن لا يتجاوز 500 حرف")]
    public string? Address { get; set; }

    /// <summary>الرصيد المالي الافتتاحي المستحق للمورد</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي 0")]
    [MoneyAmount]
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>قائمة أرقام هواتف المورد</summary>
    [NoNullItems]
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}
