using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات الموردين وهواتفهم وحركات كشف حساباتهم.
/// </summary>
public partial class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    /// <summary>
    /// تهيئة مستودع الموردين بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public SupplierRepository(AppDbContext context) : base(context)
    {
    }

    // ── هواتف المورد (Supplier Phones) ──────────────────────────

    /// <inheritdoc/>
    public async Task AddPhoneAsync(SupplierPhone phone, CancellationToken ct = default)
    {
        // إضافة رقم هاتف جديد للمورد إلى قاعدة البيانات
        await _context.Set<SupplierPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<SupplierPhone> phones, CancellationToken ct = default)
    {
        // إضافة قائمة أرقام هواتف للمورد دفعة واحدة
        await _context.Set<SupplierPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void UpdatePhone(SupplierPhone phone)
    {
        // تحديث رقم هاتف المورد مع تطبيق حماية المفاتيح والتزامن
        EntityPersistence.Update(_context, phone);
    }

    /// <inheritdoc/>
    public void RemovePhone(SupplierPhone phone)
    {
        // إزالة رقم هاتف المورد نهائياً من السياق وقاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void SoftDeletePhone(SupplierPhone phone)
    {
        // تطبيق الحذف المنطقي على رقم هاتف المورد
        EntityPersistence.SoftDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(SupplierPhone phone)
    {
        // حذف رقم هاتف المورد نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<SupplierPhone?> GetPhoneByIdAsync(Guid phoneId, CancellationToken ct = default)
    {
        // استرجاع هاتف المورد ككيان متتبع للتعديل
        return await EntityPersistence.GetByIdAsync<SupplierPhone>(_context, phoneId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierPhone>> GetPhonesAsync(Guid supplierId, CancellationToken ct = default)
    {
        // استعلام هواتف المورد كقراءة فقط بدون تتبع
        return await _context.Set<SupplierPhone>()
            .AsNoTracking()
            .Where(p => p.SupplierId == supplierId)
            .ToListAsync(ct);
    }

    // ── حركات كشف حساب المورد (Supplier Transactions) ───────────

    /// <inheritdoc/>
    public async Task AddTransactionAsync(SupplierTransaction transaction, CancellationToken ct = default)
    {
        // إضافة حركة مالية في كشف حساب المورد
        await _context.Set<SupplierTransaction>().AddAsync(transaction, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsAsync(Guid supplierId, CancellationToken ct = default)
    {
        // استرجاع حركات كشف حساب المورد مرتبة تنازلياً حسب تاريخ الحركة
        return await _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Where(t => t.SupplierId == supplierId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SupplierTransaction>> GetTransactionsByDateRangeAsync(Guid supplierId, DateTime? fromDate, DateTime? toDate, Guid? branchId = null, CancellationToken ct = default)
    {
        // تجهيز استعلام كشف حساب المورد متضمناً بيانات الفرع وبدون تتبع
        var query = _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Include(t => t.Branch)
            .Where(t => t.SupplierId == supplierId);

        // تطبيق فلتر بداية الفترة إن وجد
        if (fromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= fromDate.Value);

        // تطبيق فلتر نهاية الفترة مع معالجة توقيت منتصف الليل لشمول اليوم كاملاً
        if (toDate.HasValue)
        {
            var end = toDate.Value;
            if (end.TimeOfDay == TimeSpan.Zero && end.Date < DateTime.MaxValue.Date)
            {
                // إضافة يوم واستخدام شرط الأصغر تماماً لشمول حركات اليوم حتى 23:59:59
                var exclusiveEnd = end.AddDays(1);
                query = query.Where(t => t.TransactionDate < exclusiveEnd);
            }
            else
            {
                // ضبط التاريخ الأقصى إذا صادف منتصف الليل
                if (end.TimeOfDay == TimeSpan.Zero) end = DateTime.MaxValue;
                query = query.Where(t => t.TransactionDate <= end);
            }
        }

        // تطبيق فلتر الفرع إن تم تحديده
        if (branchId.HasValue)
            query = query.Where(t => t.BranchId == branchId.Value);

        // ترتيب الحركات تصاعدياً حسب تاريخ الحركة ثم تاريخ الإنشاء والمعرف
        return await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ThenBy(t => t.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetBalanceBeforeDateAsync(Guid supplierId, DateTime fromDate, CancellationToken ct = default)
    {
        // استخراج الرصيد الافتتاحي للمورد
        var supplier = await _context.Set<Supplier>()
            .AsNoTracking()
            .Where(s => s.Id == supplierId)
            .Select(s => new { s.OpeningBalance })
            .FirstOrDefaultAsync(ct);

        // حساب صافي التغير في رصيد المورد (الدائن ناقص المدين) للحركات السابقة للتاريخ
        var delta = await _context.Set<SupplierTransaction>()
            .AsNoTracking()
            .Where(t => t.SupplierId == supplierId && t.TransactionDate < fromDate)
            .SumAsync(t => (decimal?)(t.CreditAmount - t.DebitAmount), ct) ?? 0m;

        // إرجاع مجموع الرصيد الافتتاحي وفارق الحركات السابقة
        return (supplier?.OpeningBalance ?? 0m) + delta;
    }
}
