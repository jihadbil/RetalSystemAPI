using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الموردين (Suppliers) وإدارة هواتفهم وحركات حساباتهم كوحدة متكاملة.
/// </summary>
public interface ISupplierRepository : IRepository<Supplier>
{
    // ── هواتف المورد (Supplier Phones) ──────────────────────────

    /// <summary>إضافة رقم هاتف جديد للمورد</summary>
    Task AddPhoneAsync(SupplierPhone phone, CancellationToken ct = default);

    /// <summary>إضافة مجموعة أرقام هواتف للمورد</summary>
    Task AddPhonesAsync(IEnumerable<SupplierPhone> phones, CancellationToken ct = default);

    /// <summary>تحديث رقم هاتف مورد</summary>
    void UpdatePhone(SupplierPhone phone);

    /// <summary>إزالة رقم هاتف من السياق المحلي</summary>
    void RemovePhone(SupplierPhone phone);

    /// <summary>حذف ناعم لرقم هاتف مورد</summary>
    void SoftDeletePhone(SupplierPhone phone);

    /// <summary>حذف نهائي لرقم هاتف مورد</summary>
    void HardDeletePhone(SupplierPhone phone);

    /// <summary>جلب هاتف مورد بواسطة معرفه</summary>
    Task<SupplierPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default);

    /// <summary>جلب كافة هواتف مورد محدد</summary>
    Task<IReadOnlyList<SupplierPhone>> GetPhonesAsync(Guid supplierId, CancellationToken ct = default);


    // ── حركات كشف حساب المورد (Supplier Transactions) ───────────

    /// <summary>إضافة حركة جديدة في كشف حساب المورد</summary>
    Task AddTransactionAsync(SupplierTransaction transaction, CancellationToken ct = default);

    /// <summary>جلب حركات كشف حساب مورد محدد</summary>
    Task<IReadOnlyList<SupplierTransaction>> GetTransactionsAsync(Guid supplierId, CancellationToken ct = default);

    /// <summary>جلب حركات كشف حساب مورد خلال فترة زمنية محددة مرتبة تصاعدياً</summary>
    Task<IReadOnlyList<SupplierTransaction>> GetTransactionsByDateRangeAsync(
        Guid supplierId,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? branchId = null,
        CancellationToken ct = default);

    /// <summary>جلب رصيد المورد في اللحظة السابقة لتاريخ محدد (الرصيد الافتتاحي لكشف الحساب)</summary>
    Task<decimal> GetBalanceBeforeDateAsync(Guid supplierId, DateTime fromDate, CancellationToken ct = default);
}
