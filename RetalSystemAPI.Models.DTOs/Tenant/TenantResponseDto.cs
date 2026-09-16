using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class TenantResponseDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // ── البيانات الرسمية والقانونية ────────────────────────────
    public string? CommercialName { get; set; }
    public string? TaxNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    public string? BusinessType { get; set; }

    // ── بيانات الاتصال والعناوين المتقدمة ──────────────────────
    public string? AdditionalPhone { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // ── الهوية البصرية وإعدادات المطبوعات والفواتير ───────────
    public string? InvoiceHeaderNote { get; set; }
    public string? InvoiceFooterNote { get; set; }
    public string? BankDetails { get; set; }

    // ── الإعدادات المالية والإقليمية ──────────────────────────
    public string DefaultCurrency { get; set; } = "دينار ليبي";
    public string DefaultCurrencySymbol { get; set; } = "د.ل";
    public decimal DefaultTaxRate { get; set; }
    public bool IsTaxIncludedInPrices { get; set; }
    public string Timezone { get; set; } = "Africa/Tripoli";

    // ── ملخص الإحصائيات (Counts Summary) ─────────────────────
    public int BranchesCount { get; set; }
    public int WarehousesCount { get; set; }
    public int UsersCount { get; set; }
}
