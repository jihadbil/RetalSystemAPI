using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Desktop.Models.Suppliers;

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}

public class SupplierSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public int PhoneCount { get; set; }
}

public class SupplierPhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? Name { get; set; }
}

public class CreateSupplierRequest
{
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "العنوان لا يمكن أن يتجاوز 200 حرف")]
    public string? Address { get; set; }

    public decimal OpeningBalance { get; set; }
    public List<CreateSupplierPhoneRequest> Phones { get; set; } = new();
}

public class CreateSupplierPhoneRequest
{
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [MaxLength(20, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 20 حرفاً")]
    public string PhoneNumber { get; set; } = null!;

    [MaxLength(100, ErrorMessage = "اسم جهة الاتصال لا يمكن أن يتجاوز 100 حرف")]
    public string? Name { get; set; }
}

public class UpdateSupplierRequest
{
    [Required(ErrorMessage = "اسم المورد مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "العنوان لا يمكن أن يتجاوز 200 حرف")]
    public string? Address { get; set; }

    public decimal OpeningBalance { get; set; }
}

public enum SupplierTransactionType
{
    OpeningBalance = 0,
    PurchaseInvoice = 1,
    Payment = 2,
    PurchaseReturn = 3,
    Adjustment = 4
}

public class SupplierStatementDto
{
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public decimal OpeningBalance { get; set; }
    public decimal BalanceBeforePeriod { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<SupplierTransactionDto> Transactions { get; set; } = new();
}

public class SupplierTransactionDto
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public SupplierTransactionType TransactionType { get; set; }
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

public class CreateSupplierPaymentRequest
{
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid SupplierId { get; set; }

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

public class CreateSupplierAdjustmentRequest
{
    [Required(ErrorMessage = "معرف المورد مطلوب")]
    public Guid SupplierId { get; set; }

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
