using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public interface IRepository<T>
    where T : BaseEntity
{
    /// <summary>قراءة دون تتبع، مع احترام عزل المستأجر والحذف المنطقي.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>تحميل نسخة متتبعة للتعديل ضمن وحدة العمل الحالية.</summary>
    Task<T?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void SoftDelete(T entity);
}
