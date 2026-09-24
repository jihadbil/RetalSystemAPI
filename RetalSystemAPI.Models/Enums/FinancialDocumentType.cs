namespace RetalSystemAPI.Models.Enums;

/// <summary>
/// يحدد نوع المستند المالي المرتبط بالحركات المالية لضمان التكامل وقابلية التتبع المحاسبي.
/// </summary>
// تعريف تعداد أنواع المستندات المالية في النظام
public enum FinancialDocumentType
{
    /// <summary>
    /// فاتورة مبيعات صادرة للعميل.
    /// </summary>
    // مستند فاتورة مبيعات
    SalesInvoice = 1,

    /// <summary>
    /// إشعار مرتجع مبيعات وبضاعة مستردة.
    /// </summary>
    // مستند مرتجع مبيعات
    SalesReturn = 2,

    /// <summary>
    /// فاتورة مشتريات واردة من المورد.
    /// </summary>
    // مستند فاتورة مشتريات
    PurchaseInvoice = 3,

    /// <summary>
    /// إشعار مرتجع مشتريات وبضاعة معادة للمورد.
    /// </summary>
    // مستند مرتجع مشتريات
    PurchaseReturn = 4,

    /// <summary>
    /// سند صرف مصروف تشغيلي أو إداري.
    /// </summary>
    // مستند سند مصروف
    Expense = 5,

    /// <summary>
    /// سند قبض دفعة وسداد نقدي أو بنكي من عميل.
    /// </summary>
    // مستند سداد وقبض من عميل
    CustomerPayment = 6,

    /// <summary>
    /// سند صرف دفعة وسداد لمورد.
    /// </summary>
    // مستند سداد وصرف لمورد
    SupplierPayment = 7,

    /// <summary>
    /// سند تسوية مالية يدوية.
    /// </summary>
    // مستند تسوية مالية
    Adjustment = 8,

    /// <summary>
    /// قيد رصيد افتتاحي لحساب مالي أو خزينة.
    /// </summary>
    // مستند رصيد افتتاحي
    OpeningBalance = 9,

    /// <summary>
    /// سند تحويل ومناقلة نقدية بين الخزائن والحسابات البنكية.
    /// </summary>
    // مستند تحويل بين خزائن
    SafeTransfer = 10,

    /// <summary>
    /// عملية افتتاح وردية وجلسة درج نقود الكاشير.
    /// </summary>
    // مستند افتتاح وردية كاشير
    CashSessionOpening = 11,

    /// <summary>
    /// عملية إغلاق وردية وجلسة درج نقود الكاشير.
    /// </summary>
    // مستند إغلاق وردية كاشير
    CashSessionClosing = 12,

    /// <summary>
    /// فرق جرد نقدي معتمد (عجز أو زيادة) لدرج الكاشير.
    /// </summary>
    // مستند فرق جرد نقدية
    CashVariance = 13
}
