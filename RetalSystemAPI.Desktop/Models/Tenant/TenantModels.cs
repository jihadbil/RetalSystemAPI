using System;

namespace RetalSystemAPI.Desktop.Models.Tenant;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string FullLogoUrl => string.IsNullOrWhiteSpace(LogoUrl)
        ? string.Empty
        : (LogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? LogoUrl
            : $"https://localhost:7226/{LogoUrl.TrimStart('/')}");
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── البيانات الرسمية والقانونية ────────────────────────────
    public string? CommercialName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? BusinessType { get; set; }

    // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────
    public string? AdditionalPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "ليبيا";
    public string? PostalCode { get; set; }

    // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────
    public string? InvoiceHeaderNote { get; set; }
    public string? InvoiceFooterNote { get; set; }
    public string? BankDetails { get; set; }

    // ── الإعدادات المالية والإقليمية ──────────────────────────
    public string DefaultCurrency { get; set; } = "دينار ليبي";
    public string DefaultCurrencySymbol { get; set; } = "د.ل";
    public decimal DefaultTaxRate { get; set; } = 0m;
    public bool IsTaxIncludedInPrices { get; set; } = false;
    public string Timezone { get; set; } = "Africa/Tripoli";

    // ── ملخص الإحصائيات (Counts Summary) ─────────────────────
    public int BranchesCount { get; set; }
    public int WarehousesCount { get; set; }
    public int UsersCount { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string? CommercialName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? BusinessType { get; set; }
    public string? AdditionalPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "ليبيا";
    public string? PostalCode { get; set; }
    public string? InvoiceHeaderNote { get; set; }
    public string? InvoiceFooterNote { get; set; }
    public string? BankDetails { get; set; }
    public string DefaultCurrency { get; set; } = "دينار ليبي";
    public string DefaultCurrencySymbol { get; set; } = "د.ل";
    public decimal DefaultTaxRate { get; set; } = 0m;
    public bool IsTaxIncludedInPrices { get; set; } = false;
    public string Timezone { get; set; } = "Africa/Tripoli";
}

public class UpdateTenantRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? CommercialName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? BusinessType { get; set; }
    public string? AdditionalPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "ليبيا";
    public string? PostalCode { get; set; }
    public string? InvoiceHeaderNote { get; set; }
    public string? InvoiceFooterNote { get; set; }
    public string? BankDetails { get; set; }
    public string DefaultCurrency { get; set; } = "دينار ليبي";
    public string DefaultCurrencySymbol { get; set; } = "د.ل";
    public decimal DefaultTaxRate { get; set; } = 0m;
    public bool IsTaxIncludedInPrices { get; set; } = false;
    public string Timezone { get; set; } = "Africa/Tripoli";
}
