using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Branch;

public class BranchPhoneDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsDefault { get; set; }
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BranchPhoneDto> Phones { get; set; } = new();
    public List<string> PhoneNumbers { get; set; } = new();
    public string? PrimaryPhone => Phones.Count > 0 ? Phones[0].PhoneNumber : (PhoneNumbers.Count > 0 ? PhoneNumbers[0] : null);
}

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<BranchPhoneDto> Phones { get; set; } = new();
    public List<string> PhoneNumbers { get; set; } = new();
}

public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public List<BranchPhoneDto> Phones { get; set; } = new();
    public List<string> PhoneNumbers { get; set; } = new();
}
