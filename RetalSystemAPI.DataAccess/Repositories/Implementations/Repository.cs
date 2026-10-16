using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public abstract class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => EntityPersistence.GetReadOnlyByIdAsync<T>(_context, id, ct);
    public Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default) => EntityPersistence.GetByIdAsync<T>(_context, id, ct);
    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default) => _dbSet.AnyAsync(e => e.Id == id, ct);
    public async Task AddAsync(T entity, CancellationToken ct = default) => await _dbSet.AddAsync(entity, ct);
    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) => _dbSet.AddRangeAsync(entities, ct);
    public void Update(T entity) => EntityPersistence.Update(_context, entity);
    public void SoftDelete(T entity) => EntityPersistence.SoftDelete(_context, entity);
}
