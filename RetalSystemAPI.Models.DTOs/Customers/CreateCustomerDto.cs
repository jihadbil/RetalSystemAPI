using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Customers;

public class CreateCustomerDto
{
    [Required(ErrorMessage = "اسم العميل مطلوب")]
    [MaxLength(100, ErrorMessage = "اسم العميل يجب أن لا يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    [MaxLength(50, ErrorMessage = "كود العميل يجب أن لا يتجاوز 50 حرف")]
    public string? Code { get; set; }

    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب أن لا يتجاوز 100 حرف")]
    public string? Email { get; set; }

    [MaxLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
    public string? Address { get; set; }

    public CustomerType Type { get; set; } = CustomerType.Retail;

    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي يجب أن يكون أكبر من أو يساوي 0")]
    public decimal OpeningBalance { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "سقف الدين يجب أن يكون أكبر من أو يساوي 0")]
    public decimal CreditLimit { get; set; } = 0;

    public List<CustomerPhoneDto> Phones { get; set; } = new();
}
