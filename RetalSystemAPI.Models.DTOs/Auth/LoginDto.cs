using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

/// <summary>
/// ناقل بيانات طلب تسجيل الدخول (Login Request DTO).
/// يستقبل بيانات الاعتماد (اسم المستخدم وكلمة المرور) مع التحقق من صحتها.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// اسم المستخدم المسجل في النظام.
    /// </summary>
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// كلمة المرور الخاصة بحساب المستخدم.
    /// </summary>
    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = null!;
}
