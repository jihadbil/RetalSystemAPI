using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Pos;

/// <summary>
/// ناقل بيانات تسجيل دفعة مالية واحدة في نقطة البيع مع التحقق من صحة المدفوعات المجزأة.
/// </summary>
public class PosPaymentDto : IValidatableObject
{
    /// <summary>معرف حركة العميل المولد محلياً لضمان عدم تكرار الدفع (Idempotency Key)</summary>
    [NotEmptyGuid] public Guid ClientMovementId { get; set; }

    /// <summary>طريقة الدفع (نقدي، شبكة / بطاقة، تحويل بنكي، شيك)</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>المبلغ الصافي المخصص لسداد الفاتورة بهذه الطريقة</summary>
    [MoneyAmount] public decimal Amount { get; set; }

    /// <summary>المبلغ النقدي المسلّم فعلياً من العميل (لحساب المتبقي / الفكة)</summary>
    [MoneyAmount] public decimal? TenderedAmount { get; set; }

    /// <summary>
    /// إجراء التحقق المخصص من تناسق وسيلة الدفع والمبالغ المقدمة.
    /// </summary>
    /// <param name="validationContext">سياق التحقق من صحة الكائن.</param>
    /// <returns>قائمة أخطاء التحقق إن وجدت.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // التحقق من أن طريقة الدفع تنتمي لوسائل القبض الفعلية المعتمدة في نقاط البيع
        if (PaymentMethod is not (PaymentMethod.Cash or PaymentMethod.CreditCard or PaymentMethod.BankTransfer or PaymentMethod.Cheque))
            yield return new ValidationResult("اختر وسيلة قبض فعلية", [nameof(PaymentMethod)]);

        // التحقق من أن المبلغ المخصص لهذه الوسيلة موجب وأكبر من الصفر
        if (Amount <= 0)
            yield return new ValidationResult("مبلغ كل وسيلة يجب أن يكون أكبر من صفر", [nameof(Amount)]);

        // التحقق من أن المبلغ المستلم لا يقل عن المطلوب، وأنه لا ينطبق إلا على الدفع النقدي
        if (TenderedAmount.HasValue && (PaymentMethod != PaymentMethod.Cash || TenderedAmount < Amount))
            yield return new ValidationResult("المستلم النقدي يجب ألا يقل عن النقد المخصص للفاتورة", [nameof(TenderedAmount)]);
    }
}

/// <summary>
/// ناقل بيانات استجابة سداد دفعة نقطة البيع.
/// </summary>
public class PosPaymentResponseDto
{
    /// <summary>المعرف الفريد لسجل الدفعة</summary>
    public Guid Id { get; set; }

    /// <summary>طريقة الدفع المستخدمة</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>مبلغ الدفعة الصافي</summary>
    public decimal Amount { get; set; }

    /// <summary>المبلغ المستلم باليد في حالة النقد</summary>
    public decimal? TenderedAmount { get; set; }

    /// <summary>هل تم تسوية وتوريد الدفعة إلى الخزائن الرسمية</summary>
    public bool IsSettled { get; set; }
}

/// <summary>
/// ناقل بيانات تسوية وتوريد مدفوعات وردية الكاشير إلى الخزائن وحسابات البنوك.
/// </summary>
public class SettleCashSessionPaymentsDto
{
    /// <summary>قائمة ربط كل وسيلة دفع بالخزينة أو الحساب البنكي المستهدف للتوريد</summary>
    [Required, MinLength(1), MaxLength(4), NoNullItems]
    public List<PosSettlementAccountDto> Accounts { get; set; } = [];

    /// <summary>ملاحظات إضافية حول عملية التوريد والتسوية</summary>
    [MaxLength(500)] public string? Notes { get; set; }
}

/// <summary>
/// ناقل بيانات تحديد وجهة توريد وسيلة دفع معينة إلى خزينة محددة.
/// </summary>
public class PosSettlementAccountDto
{
    /// <summary>وسيلة الدفع المراد توريد حصيلتها</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>معرف الخزينة أو الحساب البنكي المستلم</summary>
    [NotEmptyGuid] public Guid SafeId { get; set; }
}

/// <summary>
/// ناقل بيانات تقرير وتفاصيل مقبوضات جلسة الكاشير للتسوية.
/// </summary>
public class CashSessionPaymentsDto
{
    /// <summary>معرف جلسة الكاشير</summary>
    public Guid Id { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم نقطة البيع</summary>
    public string TerminalName { get; set; } = "";

    /// <summary>اسم درج النقدية</summary>
    public string CashDrawerName { get; set; } = "";

    /// <summary>تاريخ ووقت فتح الجلسة</summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>حالة الجلسة الحالية</summary>
    public CashSessionStatus Status { get; set; }

    /// <summary>هل اكتملت تسوية جميع مدفوعات الجلسة</summary>
    public bool IsSettled { get; set; }

    /// <summary>إجماليات المبالغ المحصلة موزعة حسب طريقة الدفع</summary>
    public List<CashSessionPaymentTotalDto> Totals { get; set; } = [];

    /// <summary>قائمة الحركات الفردية المسجلة خلال الجلسة</summary>
    public List<CashSessionMovementDto> Movements { get; set; } = [];
}

/// <summary>
/// ناقل بيانات إجمالي المقبوضات لوسيلة دفع معينة في الجلسة.
/// </summary>
public class CashSessionPaymentTotalDto
{
    /// <summary>وسيلة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>إجمالي المبلغ المحصل بهذه الوسيلة</summary>
    public decimal Amount { get; set; }

    /// <summary>معرف الخزينة المورد إليها (إن تم التسوية)</summary>
    public Guid? SafeId { get; set; }

    /// <summary>اسم الخزينة المورد إليها</summary>
    public string? SafeName { get; set; }
}

/// <summary>
/// ناقل بيانات تفاصيل حركة دفع / رد فردية في جلسة الكاشير.
/// </summary>
public class CashSessionMovementDto
{
    /// <summary>معرف الحركة</summary>
    public Guid Id { get; set; }

    /// <summary>رقم الفاتورة المرتبطة</summary>
    public string InvoiceNumber { get; set; } = "";

    /// <summary>وسيلة الدفع</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>اتجاه الحركة (قبض / سداد وارد، أو صرف / رد صادر)</summary>
    public TransactionDirection Direction { get; set; }

    /// <summary>مبلغ الحركة</summary>
    public decimal Amount { get; set; }

    /// <summary>حالة تسوية الحركة</summary>
    public bool IsSettled { get; set; }
}
