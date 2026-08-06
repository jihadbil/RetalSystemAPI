using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "معرف المستأجر مطلوب")]
    public Guid TenantId { get; set; }

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }
}
