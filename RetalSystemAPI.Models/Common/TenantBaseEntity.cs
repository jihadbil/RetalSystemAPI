namespace RetalSystemAPI.Models.Common;

/// <summary>
/// الكيان الأساسي لجميع النماذج التابعة لمستأجر معين في النظام.
/// </summary>
public abstract class TenantBaseEntity : BaseEntity
{
    /// <summary>
    /// معرف المستأجر الذي ينتمي إليه الكيان.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// الكائن المرجعي للمستأجر.
    /// </summary>
    public Tenant? Tenant { get; set; }
}

