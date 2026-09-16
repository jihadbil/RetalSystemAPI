using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RetalSystemAPI.Desktop.Models.Customers;

public enum CustomerType
{
    Regular = 0,
    Vip = 1,
    Wholesale = 2,
    Corporate = 3
}

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type switch
    {
        CustomerType.Regular => "عادي",
        CustomerType.Vip => "مميز (VIP)",
        CustomerType.Wholesale => "جملة",
        CustomerType.Corporate => "شركة / مؤسسة",
        _ => "عادي"
    };
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }

    [JsonPropertyName("phones")]
    public List<CustomerPhoneDto> Phones { get; set; } = new();

    [JsonIgnore]
    public List<CustomerPhoneDto> CustomerPhones
    {
        get => Phones;
        set => Phones = value;
    }
}

public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type switch
    {
        CustomerType.Regular => "عادي",
        CustomerType.Vip => "مميز (VIP)",
        CustomerType.Wholesale => "جملة",
        CustomerType.Corporate => "شركة / مؤسسة",
        _ => "عادي"
    };
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public int PhoneCount { get; set; }
    public string? PrimaryPhone { get; set; }
}

public class CustomerPhoneDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string? ContactName { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateCustomerRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public decimal CreditLimit { get; set; }
    public List<CreateCustomerPhoneRequest> Phones { get; set; } = new();
}

public class CreateCustomerPhoneRequest
{
    public string PhoneNumber { get; set; } = null!;
    public string? ContactName { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateCustomerRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public CustomerType Type { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public List<CreateCustomerPhoneRequest> Phones { get; set; } = new();
}
