using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Branch;

public class BranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
}

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
}

public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
}
