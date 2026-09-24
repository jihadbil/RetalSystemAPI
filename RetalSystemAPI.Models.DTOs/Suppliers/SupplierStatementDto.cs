using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Suppliers;

/// <summary>
/// ناقل بيانات التقرير المالي المجمع لكشف حساب المورد (Supplier Statement Report DTO).
/// </summary>
public class SupplierStatementDto
{
    /// <summary>معرف المورد</summary>
    public Guid SupplierId { get; set; }

    /// <summary>اسم المورد أو الشركة</summary>
    public string SupplierName { get; set; } = null!;

    /// <summary>الرصيد الافتتاحي للمورد</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>الرصيد قبل بداية الفترة المحددة</summary>
    public decimal BalanceBeforePeriod { get; set; }

    /// <summary>إجمالي الحركات المدينة خلال الفترة (سدادات ومرتجعات مشتريات)</summary>
    public decimal TotalDebit { get; set; }

    /// <summary>إجمالي الحركات الدائنة خلال الفترة (فواتير مشتريات ومطالبات)</summary>
    public decimal TotalCredit { get; set; }

    /// <summary>الرصيد الحالي النهائي المستحق للمورد</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>تاريخ بداية فترة التقرير</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية فترة التقرير</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>قائمة حركات كشف الحساب بالتسلسل الزمني</summary>
    public List<SupplierTransactionResponseDto> Transactions { get; set; } = new();
}
