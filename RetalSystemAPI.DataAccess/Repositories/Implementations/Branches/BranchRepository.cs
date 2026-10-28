using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات الفروع وإدارة هواتفها التابعة.
/// </summary>
public partial class BranchRepository : Repository<Branch>, IBranchRepository
{
    /// <summary>
    /// تهيئة مستودع الفروع بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public BranchRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddPhoneAsync(BranchPhone phone, CancellationToken ct = default)
    {
        // إضافة سجل رقم الهاتف الجديد إلى جدول هواتف الفروع
        await _context.Set<BranchPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<BranchPhone> phones, CancellationToken ct = default)
    {
        // إضافة قائمة أرقام الهواتف الجديدة دفعة واحدة
        await _context.Set<BranchPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void RemovePhone(BranchPhone phone)
    {
        // إزالة رقم الهاتف فيزيائياً من السياق وقاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(BranchPhone phone)
    {
        // حذف رقم الهاتف نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void SoftDeletePhone(BranchPhone phone)
    {
        // تطبيق الحذف المنطقي على رقم الهاتف
        EntityPersistence.SoftDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BranchPhone>> GetPhonesAsync(Guid branchId, CancellationToken ct = default)
    {
        // جلب قائمة هواتف الفرع المحدد كقراءة فقط بدون تتبع
        return await _context.Set<BranchPhone>()
            .AsNoTracking()
            .Where(p => p.BranchId == branchId)
            .ToListAsync(ct);
    }
}
