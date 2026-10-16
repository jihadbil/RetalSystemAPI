using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات الفروع وإدارة هواتفها.
/// </summary>
public partial class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public async Task AddPhoneAsync(BranchPhone phone, CancellationToken ct = default)
    {
        await _context.Set<BranchPhone>().AddAsync(phone, ct);
    }

    /// <inheritdoc/>
    public async Task AddPhonesAsync(IEnumerable<BranchPhone> phones, CancellationToken ct = default)
    {
        await _context.Set<BranchPhone>().AddRangeAsync(phones, ct);
    }

    /// <inheritdoc/>
    public void RemovePhone(BranchPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public void HardDeletePhone(BranchPhone phone)
    {
        EntityPersistence.HardDelete(_context, phone);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BranchPhone>> GetPhonesAsync(Guid branchId, CancellationToken ct = default)
    {
        return await _context.Set<BranchPhone>().AsNoTracking().Where(p => p.BranchId == branchId).ToListAsync(ct);
    }
}
