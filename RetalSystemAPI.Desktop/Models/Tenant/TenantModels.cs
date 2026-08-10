using System;

namespace RetalSystemAPI.Desktop.Models.Tenant;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
}
