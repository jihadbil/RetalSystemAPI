using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RetalSystemAPI.Desktop.Models.Customers;

public enum CustomerType
{
    Regular = 0,
    Vip = 1,
    Wholesale = 2,
    Corporate = 3
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type switch
    {
        CustomerType.Regular => "عادي",
        CustomerType.Vip => "مميز (VIP)",
        CustomerType.Wholesale => "جملة",
        CustomerType.Corporate => "شركة / مؤسسة",
        _ => "عادي"
    };
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }

    [JsonPropertyName("phones")]
    public List<CustomerPhoneDto> Phones { get; set; } = new();

    [JsonIgnore]
    public List<CustomerPhoneDto> CustomerPhones
    {
        get => Phones;
        set => Phones = value;
    }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type switch
    {
        CustomerType.Regular => "عادي",
        CustomerType.Vip => "مميز (VIP)",
        CustomerType.Wholesale => "جملة",
        CustomerType.Corporate => "شركة / مؤسسة",
        _ => "عادي"
    };
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public int PhoneCount { get; set; }
    public string? PrimaryPhone { get; set; }
}

public class CustomerPhoneDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? ContactName { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public decimal CreditLimit { get; set; }
    public List<CreateCustomerPhoneRequest> Phones { get; set; } = new();
}

public class CreateCustomerPhoneRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string? ContactName { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public List<CreateCustomerPhoneRequest> Phones { get; set; } = new();
}

public enum CustomerTransactionType
{
    OpeningBalance = 0,
    SalesInvoice = 1,
    Payment = 2,
    SalesReturn = 3,
    Adjustment = 4
}

public class CustomerStatementDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerCode { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal BalanceBeforePeriod { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<CustomerTransactionDto> Transactions { get; set; } = new();
}

public class CustomerTransactionDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerTransactionType TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public RetalSystemAPI.Desktop.Models.Accounting.FinancialDocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public RetalSystemAPI.Desktop.Models.Accounting.PaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class CreateCustomerPaymentRequest
{
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    public Guid CustomerId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ السداد يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    public RetalSystemAPI.Desktop.Models.Accounting.PaymentMethod PaymentMethod { get; set; } = RetalSystemAPI.Desktop.Models.Accounting.PaymentMethod.Cash;

    public Guid? SafeId { get; set; }

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

public class CreateCustomerAdjustmentRequest
{
    [Required(ErrorMessage = "معرف العميل مطلوب")]
    public Guid CustomerId { get; set; }

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "اتجاه التسوية مطلوب")]
    public RetalSystemAPI.Desktop.Models.Accounting.TransactionDirection Direction { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ التسوية يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "سبب وبيان التسوية مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}
