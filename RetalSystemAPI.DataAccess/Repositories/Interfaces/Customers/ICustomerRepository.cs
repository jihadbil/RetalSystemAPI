using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات العملاء والزبائن (Customers) وإدارة هواتفهم وحركات حساباتهم كوحدة متكاملة.
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    // ── هواتف العميل (Customer Phones) ──────────────────────────

    /// <summary>إضافة رقم هاتف جديد للعميل</summary>
    Task AddPhoneAsync(CustomerPhone phone, CancellationToken ct = default);

    /// <summary>إضافة مجموعة أرقام هواتف للعميل</summary>
    Task AddPhonesAsync(IEnumerable<CustomerPhone> phones, CancellationToken ct = default);

    /// <summary>تحديث رقم هاتف عميل</summary>
    void UpdatePhone(CustomerPhone phone);

    /// <summary>إزالة رقم هاتف من السياق المحلي</summary>
    void RemovePhone(CustomerPhone phone);

    /// <summary>حذف ناعم لرقم هاتف عميل</summary>
    void SoftDeletePhone(CustomerPhone phone);

    /// <summary>حذف نهائي لرقم هاتف عميل</summary>
    void HardDeletePhone(CustomerPhone phone);

    /// <summary>جلب هاتف عميل بواسطة معرفه</summary>
    Task<CustomerPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default);

    /// <summary>جلب كافة هواتف عميل محدد</summary>
    Task<IReadOnlyList<CustomerPhone>> GetPhonesAsync(Guid customerId, CancellationToken ct = default);


    // ── حركات كشف حساب العميل (Customer Transactions) ───────────

    /// <summary>إضافة حركة جديدة في كشف حساب العميل</summary>
    Task AddTransactionAsync(CustomerTransaction transaction, CancellationToken ct = default);

    /// <summary>جلب حركات كشف حساب عميل محدد</summary>
    Task<IReadOnlyList<CustomerTransaction>> GetTransactionsAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>جلب حركات كشف حساب عميل خلال فترة زمنية محددة مرتبة تصاعدياً</summary>
    Task<IReadOnlyList<CustomerTransaction>> GetTransactionsByDateRangeAsync(
        Guid customerId,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? branchId = null,
        CancellationToken ct = default);

    /// <summary>جلب رصيد العميل في اللحظة السابقة لتاريخ محدد (الرصيد الافتتاحي لكشف الحساب)</summary>
    Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime fromDate, CancellationToken ct = default);
}
