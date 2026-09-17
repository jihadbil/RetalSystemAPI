using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

/// <summary>
/// ناقل بيانات تسجيل مستخدم جديد (Register User DTO).
/// يُستخدم لإنشاء حساب مستخدم وربطه بمستأجر وفرع محددين.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// اسم المستخدم المطلوب تسجيله.
    /// </summary>
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// عنوان البريد الإلكتروني للمستخدم.
    /// </summary>
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// كلمة المرور لحماية الحساب (بحد أدنى 6 أحرف).
    /// </summary>
    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// المعرف الفريد للمستأجر (الشركة/المؤسسة).
    /// </summary>
    [Required(ErrorMessage = "معرف المستأجر مطلوب")]
    public Guid TenantId { get; set; }

    /// <summary>
    /// المعرف الفريد للفرع التابع له المستخدم.
    /// </summary>
    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }
}
