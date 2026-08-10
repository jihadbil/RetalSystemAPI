using System;
using RetalSystemAPI.Models.Common;    
using RetalSystemAPI.Models.Enums;


namespace RetalSystemAPI.Models.Suppliers;
/// <summary>
/// يمتل العمليات المالية بين المورد صاحب المحل، مثل الديون والمدفوعات. يحتوي على معلومات حول المورد، نوع العملية، المبلغ، وتاريخ العملية.
/// </summary>
public class SupplierTransaction:BaseEntity
{
    /// <summary>
    /// معرف المورد الذي تم تنفيذ العملية المالية معه. يمثل العلاقة بين العملية والمورد.
    /// </summary>
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    /// <summary>
    /// تاريخ المعاملة المالية. يتم تعيينه افتراضيًا إلى الوقت الحالي (UTC) عند إنشاء العملية.
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// نوع المعاملة
    /// </summary>
    public SupplierTransactionType Type { get; set; }
    /// <summary>
    /// قيمة المعاملة المالية. القيم الموجبة تشير إلى الديون (مستحقة للمورد)، والقيم السالبة تشير إلى المدفوعات (تم دفعها للمورد).
    /// </summary>
    public decimal Amount { get; set; } // Positive = debt (due to supplier), Negative = payment (paid to supplier)

    //public long? ReferenceId { get; set; }
    //public string? ReferenceType { get; set; }

    /// <summary>
    /// ملاحظات 
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
 
 
}
