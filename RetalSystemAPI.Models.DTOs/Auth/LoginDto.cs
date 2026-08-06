using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = null!;
}
