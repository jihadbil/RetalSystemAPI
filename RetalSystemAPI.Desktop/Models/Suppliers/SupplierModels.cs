using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Suppliers;

public class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public List<SupplierPhoneDto> Phones { get; set; } = new();
}

public class SupplierSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public int PhoneCount { get; set; }
}

public class SupplierPhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? Name { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public List<CreateSupplierPhoneRequest> Phones { get; set; } = new();
}

public class CreateSupplierPhoneRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string? Name { get; set; }
}

public class UpdateSupplierRequest
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
}
