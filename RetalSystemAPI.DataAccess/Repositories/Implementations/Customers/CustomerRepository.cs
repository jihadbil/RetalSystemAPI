using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات العملاء والزبائن وهواتفهم وحركات كشف حساباتهم.
/// </summary>
public partial class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    /// <summary>
    /// تهيئة مستودع العملاء بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    // ── هواتف العميل (Customer Phones) ──────────────────────────

    /// <inheritdoc/>
    public async Task AddPhoneAsync(CustomerPhone phone, CancellationToken ct = default)
    {
        // إضافة سجل هاتف العميل إلى جدول الهواتف
        await _context.Set<CustomerPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<CustomerPhone> phones, CancellationToken ct = default)
    {
        // إضافة قائمة أرقام هواتف العميل دفعة واحدة
        await _context.Set<CustomerPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void UpdatePhone(CustomerPhone phone)
    {
        // تحديث بيانات رقم هاتف العميل مع تطبيق قيود التزامن والمفاتيح
        EntityPersistence.Update(_context, phone);
    }

    /// <inheritdoc/>
    public void RemovePhone(CustomerPhone phone)
    {
        // إزالة رقم الهاتف نهائياً من السياق وقاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void SoftDeletePhone(CustomerPhone phone)
    {
        // تطبيق الحذف المنطقي على رقم هاتف العميل
        EntityPersistence.SoftDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(CustomerPhone phone)
    {
        // حذف رقم هاتف العميل نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<CustomerPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default)
    {
        // استرجاع هاتف العميل ككيان متتبع للتعديل
        return await EntityPersistence.GetByIdAsync<CustomerPhone>(_context, phoneId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerPhone>> GetPhonesAsync(Guid customerId, CancellationToken ct = default)
    {
        // استعلام هواتف العميل كقراءة فقط بدون تتبع
        return await _context.Set<CustomerPhone>()
            .AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .ToListAsync(ct);
    }

    // ── حركات كشف حساب العميل (Customer Transactions) ───────────

    /// <inheritdoc/>
    public async Task AddTransactionAsync(CustomerTransaction transaction, CancellationToken ct = default)
    {
        // إضافة قيد حركة حساب العميل في جدول الحركات المالية
        await _context.Set<CustomerTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerTransaction>> GetTransactionsAsync(Guid customerId, CancellationToken ct = default)
    {
        // استرجاع حركات كشف حساب العميل مرتبة تنازلياً حسب تاريخ الحركة
        return await _context.Set<CustomerTransaction>()
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerTransaction>> GetTransactionsByDateRangeAsync(Guid customerId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default)
    {
        // تجهيز الاستعلام الأساسي لحركات العميل مع تضمين بيانات الفرع وبدون تتبع
        var query = _context.Set<CustomerTransaction>()
            .AsNoTracking()
            .Include(t => t.Branch)
            .Where(t => t.CustomerId == customerId);

        // تطبيق فلتر بداية الفترة إن تم تحديده
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        // تطبيق فلتر نهاية الفترة مع معالجة توقيت منتصف الليل لشمول اليوم كاملاً
        if (toDate.HasValue)
        {
            var end = toDate.Value;
            if (end.TimeOfDay == TimeSpan.Zero && end.Date < DateTime.MaxValue.Date)
            {
                // في حالة إدخال التاريخ بدون وقت، نضيف يوماً ونستخدم شرط الأصغر تماماً
                var exclusiveEnd = end.AddDays(1);
                query = query.Where(t => t.TransactionDate < exclusiveEnd);
            }
            else
            {
                // إذا كان نهاية اليوم، نضمن تاريخ أقصى
                if (end.TimeOfDay == TimeSpan.Zero) end = DateTime.MaxValue;
                query = query.Where(t => t.TransactionDate <= end);
            }
        }

        // تطبيق فلتر الفرع المحدد إن تم اختياره
        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);

        // تنفيذ الاستعلام وترتيب الحركات تصاعدياً حسب تاريخ الحركة ثم تاريخ الإنشاء والمعرف
        return await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetBalanceBeforeDateAsync(Guid customerId, DateTime fromDate, CancellationToken ct = default)
    {
        // استخراج الرصيد الافتتاحي للعميل بشكل خفيف
        var customer = await _context.Set<Customer>()
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.OpeningBalance })
            .FirstOrDefaultAsync(ct);

        // حساب صافي التغير في الرصيد (المدين ناقص الدائن) لجميع الحركات السابقة للتاريخ
        var delta = await _context.Set<CustomerTransaction>()
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId && t.TransactionDate < fromDate)
            .SumAsync(t => (decimal?)(t.DebitAmount - t.CreditAmount), ct) ?? 0m;

        // إرجاع مجموع الرصيد الافتتاحي وصافي تغير الحركات السابقة
        return (customer?.OpeningBalance ?? 0m) + delta;
    }
}
