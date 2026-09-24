using System;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Desktop.Models.Accounting;

public enum SafeType
{
    Bank = 1,
    CashBox = 2,
    CashDrawer = 3
}

public enum TransactionDirection
{
    Inbound = 1,
    Outbound = 2
}

public enum FinancialDocumentType
{
    SalesInvoice = 1,
    SalesReturn = 2,
    PurchaseInvoice = 3,
    PurchaseReturn = 4,
    Expense = 5,
    CustomerPayment = 6,
    SupplierPayment = 7,
    Adjustment = 8,
    OpeningBalance = 9,
    SafeTransfer = 10,
    CashSessionOpening = 11,
    CashSessionClosing = 12,
    CashVariance = 13
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    CreditCard = 3,
    Credit = 4,
    Cheque = 5
}

// ══════════════ SAFES & BANK ACCOUNTS ══════════════

public class SafeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public SafeType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class SafeSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public SafeType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSafeRequest
{
    [Required(ErrorMessage = "اسم الخزينة أو الحساب مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "نوع الخزينة مطلوب")]
    public SafeType Type { get; set; } = SafeType.CashBox;

    [Required(ErrorMessage = "الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "الرصيد الافتتاحي لا يمكن أن يكون سالباً")]
    public decimal OpeningBalance { get; set; }

    [MaxLength(50, ErrorMessage = "رقم الحساب المصرفي لا يمكن أن يتجاوز 50 حرفاً")]
    public string? BankAccountNumber { get; set; }

    [MaxLength(100, ErrorMessage = "اسم المصرف لا يمكن أن يتجاوز 100 حرف")]
    public string? BankName { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

public class UpdateSafeRequest
{
    [Required(ErrorMessage = "اسم الخزينة أو الحساب مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "نوع الخزينة مطلوب")]
    public SafeType Type { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(50, ErrorMessage = "رقم الحساب المصرفي لا يمكن أن يتجاوز 50 حرفاً")]
    public string? BankAccountNumber { get; set; }

    [MaxLength(100, ErrorMessage = "اسم المصرف لا يمكن أن يتجاوز 100 حرف")]
    public string? BankName { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

// ══════════════ SAFE TRANSACTIONS & TRANSFERS ══════════════

public class SafeTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = null!;
    public Guid SafeId { get; set; }
    public string SafeName { get; set; } = null!;
    public TransactionDirection Direction { get; set; }
    public string DirectionName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public FinancialDocumentType DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

public class CreateSafeTransactionRequest
{
    [Required(ErrorMessage = "الخزينة مطلوبة")]
    public Guid SafeId { get; set; }

    [Required(ErrorMessage = "اتجاه الحركة مطلوب")]
    public TransactionDirection Direction { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ الحركة يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "بيان الحركة مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Description { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

public class TransferFundsRequest
{
    [Required(ErrorMessage = "الخزينة المصدر مطلوبة")]
    public Guid FromSafeId { get; set; }

    [Required(ErrorMessage = "الخزينة الوجهة مطلوبة")]
    public Guid ToSafeId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "مبلغ التحويل يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

// ══════════════ EXPENSE CATEGORIES ══════════════

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public int ExpensesCount { get; set; }
}

public class CreateExpenseCategoryRequest
{
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;
}

public class UpdateExpenseCategoryRequest
{
    [Required(ErrorMessage = "اسم التصنيف مطلوب")]
    [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
    public string Name { get; set; } = null!;
}

// ══════════════ EXPENSES ══════════════

public class ExpenseDto
{
    public Guid Id { get; set; }
    public string ExpenseNumber { get; set; } = null!;
    public DateTime ExpenseDate { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public Guid? SafeId { get; set; }
    public string? SafeName { get; set; }
    public string? BeneficiaryName { get; set; }
    public string Description { get; set; } = null!;
    public string? Notes { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}

public class ExpenseSummaryDto
{
    public Guid Id { get; set; }
    public string ExpenseNumber { get; set; } = null!;
    public DateTime ExpenseDate { get; set; }
    public string BranchName { get; set; } = null!;
    public string ExpenseCategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public string? SafeName { get; set; }
    public string? BeneficiaryName { get; set; }
    public string Description { get; set; } = null!;
    public bool IsCancelled { get; set; }
}

public class CreateExpenseRequest
{
    [Required(ErrorMessage = "تصنيف المصروف مطلوب")]
    public Guid ExpenseCategoryId { get; set; }

    [Required(ErrorMessage = "الفرع مطلوب")]
    public Guid BranchId { get; set; }

    public Guid? SafeId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "المبلغ يجب أن يكون أكبر من الصفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

    [MaxLength(100, ErrorMessage = "اسم المستفيد لا يمكن أن يتجاوز 100 حرف")]
    public string? BeneficiaryName { get; set; }

    [Required(ErrorMessage = "البيان مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Description { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

public class UpdateExpenseRequest
{
    [Required(ErrorMessage = "تصنيف المصروف مطلوب")]
    public Guid ExpenseCategoryId { get; set; }

    [MaxLength(100, ErrorMessage = "اسم المستفيد لا يمكن أن يتجاوز 100 حرف")]
    public string? BeneficiaryName { get; set; }

    [Required(ErrorMessage = "البيان مطلوب")]
    [MaxLength(250, ErrorMessage = "البيان لا يمكن أن يتجاوز 250 حرفاً")]
    public string Description { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "الملاحظات لا يمكن أن تتجاوز 500 حرف")]
    public string? Notes { get; set; }
}

public class CancelExpenseRequest
{
    [Required(ErrorMessage = "سبب الإلغاء مطلوب")]
    [MaxLength(250, ErrorMessage = "سبب الإلغاء لا يمكن أن يتجاوز 250 حرفاً")]
    public string Reason { get; set; } = null!;
}
