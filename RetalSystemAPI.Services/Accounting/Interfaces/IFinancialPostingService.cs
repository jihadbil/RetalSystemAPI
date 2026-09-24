using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة الترحيل المالي وتحديث أرصدة الخزائن والعملاء والموردين المرتبطة بالمستندات التجارية داخل معاملة وحدة العمل.
/// </summary>
public interface IFinancialPostingService
{
    /// <summary>
    /// التحقق من صلاحية الخزينة للفرع وكفاية رصيدها للحركة المالية الصادرة أو الواردة.
    /// </summary>
    /// <param name="branchId">معرف الفرع المنفذ للحركة</param>
    /// <param name="safeId">معرف الخزينة المحدد (اختياري)</param>
    /// <param name="amount">مبلغ المعاملة المالية</param>
    /// <param name="outbound">هل الحركة صادرة (صرف) تتطلب رصيداً كافياً</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>كيان الخزينة المعتمدة أو نتيجة خطأ التحقق</returns>
    // تحديد ومطابقة الخزينة الصالحة للحركة المالية والتحقق من كفاية الرصيد
    Task<ServiceResult<Safe?>> ResolveSafeAsync(Guid branchId, Guid? safeId, decimal amount, bool outbound, CancellationToken ct = default);

    /// <summary>
    /// ترحيل حركة مالية على حساب العميل وتعديل رصيده الحالي وتسجيل سجل المعاملة.
    /// </summary>
    /// <param name="customer">كائن العميل المراد تعديل رصيده</param>
    /// <param name="balanceDelta">قيمة التغير في الرصيد (موجب أو سالب)</param>
    /// <param name="transactionType">نوع حركة العميل</param>
    /// <param name="branchId">معرف الفرع المنفذ للحركة</param>
    /// <param name="documentType">نوع المستند المالي المرتبط</param>
    /// <param name="documentId">معرف المستند المالي المرتبط</param>
    /// <param name="date">تاريخ ووقت المعاملة</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="description">الوصف والبيان المحاسبي للحركة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    // ترحيل وتعديل رصيد العميل وإنشاء سجل حركة عميل مفصل
    Task PostCustomerBalanceAsync(Customer customer, decimal balanceDelta, CustomerTransactionType transactionType,
        Guid branchId, FinancialDocumentType documentType, Guid documentId, DateTime date,
        PaymentMethod? paymentMethod, string description, CancellationToken ct = default);

    /// <summary>
    /// ترحيل حركة مالية على حساب المورد وتعديل رصيده الحالي وتسجيل سجل المعاملة.
    /// </summary>
    /// <param name="supplier">كائن المورد المراد تعديل رصيده</param>
    /// <param name="balanceDelta">قيمة التغير في الرصيد (موجب أو سالب)</param>
    /// <param name="transactionType">نوع حركة المورد</param>
    /// <param name="branchId">معرف الفرع المنفذ للحركة</param>
    /// <param name="documentType">نوع المستند المالي المرتبط</param>
    /// <param name="documentId">معرف المستند المالي المرتبط</param>
    /// <param name="date">تاريخ ووقت المعاملة</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="description">الوصف والبيان المحاسبي للحركة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    // ترحيل وتعديل رصيد المورد وإنشاء سجل حركة مورد مفصل
    Task PostSupplierBalanceAsync(Supplier supplier, decimal balanceDelta, SupplierTransactionType transactionType,
        Guid branchId, FinancialDocumentType documentType, Guid documentId, DateTime date,
        PaymentMethod? paymentMethod, string description, CancellationToken ct = default);

    /// <summary>
    /// ترحيل حركة نقدية على الخزينة أو درج النقدية وتعديل رصيدها وتسجيل سجل المعاملة النقدية.
    /// </summary>
    /// <param name="safe">كائن الخزينة أو درج النقدية</param>
    /// <param name="balanceDelta">قيمة التغير في رصيد الخزينة (موجب للقبض، سالب للصرف)</param>
    /// <param name="documentType">نوع المستند المالي المرتبط</param>
    /// <param name="documentId">معرف المستند المالي المرتبط</param>
    /// <param name="date">تاريخ ووقت الحركة النقدية</param>
    /// <param name="description">الوصف والبيان المحاسبي للحركة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <param name="cashSessionId">معرف وردية الصندوق المرتبطة إن وجدت</param>
    /// <param name="idempotencyKey">مفتاح منع تكرار العملية المحاسبية</param>
    // ترحيل وتعديل رصيد الخزينة وتسجيل حركة الصندوق
    Task PostSafeBalanceAsync(Safe safe, decimal balanceDelta, FinancialDocumentType documentType,
        Guid documentId, DateTime date, string description, CancellationToken ct = default,
        Guid? cashSessionId = null, string? idempotencyKey = null);
}
