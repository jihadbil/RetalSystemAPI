using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة إدارة وتتبع مدفوعات فواتير نقاط البيع (POS) المجزأة، وتسويتها وعكسها عند المرتجعات.
/// </summary>
public interface IPosPaymentService
{
    /// <summary>
    /// تسجيل حركات السداد المتعددة (نقداً، بطاقة، ترحيل) لفاتورة مبيعات وربطها بجلسة الصندوق النشطة.
    /// </summary>
    /// <param name="invoice">فاتورة المبيعات الصادرة</param>
    /// <param name="session">جلسة الصندوق النقدية المفتوحة لنقطة البيع</param>
    /// <param name="payments">قائمة المدفوعات المجزأة مع وسيلة السداد والمبلغ والمبلغ المقدم</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    Task RecordAsync(SalesInvoice invoice, CashSession session, IReadOnlyList<PosPaymentDto> payments, CancellationToken ct);

    /// <summary>
    /// جلب قائمة حركات الدفع المقبوضة لفاتورة مبيعات محددة مع حالة تسويتها.
    /// </summary>
    /// <param name="invoiceId">معرف فاتورة المبيعات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بتفاصيل المدفوعات المسجلة للفاتورة</returns>
    Task<List<PosPaymentResponseDto>> GetInvoicePaymentsAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// التحقق مما إذا كانت الفاتورة تمتلك حركات سداد مسجلة في جلسات الصناديق.
    /// </summary>
    /// <param name="invoiceId">معرف فاتورة المبيعات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صحيح إذا كانت الفاتورة تحتوي على مدفوعات مسجلة</returns>
    Task<bool> HasPaymentsAsync(Guid invoiceId, CancellationToken ct);

    /// <summary>
    /// عكس ورد مبالغ المدفوعات لحركة مرتجع مبيعات وخصمها من الدرج النقدي النشط.
    /// </summary>
    /// <param name="invoice">فاتورة المبيعات الأصلية المرتجع منها</param>
    /// <param name="documentType">نوع المستند المالي (مرتجع مبيعات)</param>
    /// <param name="documentId">معرف مستند المرتجع</param>
    /// <param name="amount">المبلغ الإجمالي المطلوب رده</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة توضح نجاح أو إخفاق عملية العكس المالي</returns>
    Task<ServiceResult> ReverseAsync(SalesInvoice invoice, FinancialDocumentType documentType, Guid documentId, decimal amount, CancellationToken ct);

    /// <summary>
    /// التراجع عن حركة عكس مرتجع ملغاة واستعادة المبالغ المسددة للدرج.
    /// </summary>
    /// <param name="returnId">معرف مستند المرتجع الملغى</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة العملية</returns>
    Task<ServiceResult> UndoReturnAsync(Guid returnId, CancellationToken ct);

    /// <summary>
    /// جلب قائمة جلسات الصناديق النقدية وحركات المدفوعات المرتبطة بها لفرع محدد.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات الجلسات ومدفوعاتها</returns>
    Task<ServiceResult<List<CashSessionPaymentsDto>>> GetSessionsAsync(Guid? branchId, CancellationToken ct);

    /// <summary>
    /// تسوية حركات الدفع لجلسة صندوق مغلقة وترحيل المبالغ للخزائن والحسابات المصرفية المحددة.
    /// </summary>
    /// <param name="sessionId">معرف جلسة الصندوق المراد تسويتها</param>
    /// <param name="dto">بيانات حسابات التسوية والملاحظات</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الجلسة المحدثة بعد اكتمال التسوية</returns>
    Task<ServiceResult<CashSessionPaymentsDto>> SettleAsync(Guid sessionId, SettleCashSessionPaymentsDto dto, CancellationToken ct);
}
