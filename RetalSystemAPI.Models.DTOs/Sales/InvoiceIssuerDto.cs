namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات المنشأة المصدرة للفاتورة الضريبية وفق متطلبات هيئة الزكاة والضريبة والجمارك.
/// </summary>
public sealed class InvoiceIssuerDto
{
    /// <summary>اسم المنشأة أو الشركة المصدرة</summary>
    public required string Name { get; init; }

    /// <summary>الرقم الضريبي للمنشأة (المكون من 15 رقماً)</summary>
    public string? TaxNumber { get; init; }

    /// <summary>عنوان المنشأة أو الفرع المصدر</summary>
    public string? Address { get; init; }

    /// <summary>رقم هاتف المنشأة أو الفرع</summary>
    public string? Phone { get; init; }
}
