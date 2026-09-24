using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Customers;

/// <summary>
/// ناقل بيانات التقرير المالي المجمع لكشف حساب العميل (Customer Statement Report DTO).
/// </summary>
public class CustomerStatementDto
{
    /// <summary>معرف العميل</summary>
    public Guid CustomerId { get; set; }

    /// <summary>اسم العميل</summary>
    public string CustomerName { get; set; } = null!;

    /// <summary>كود العميل</summary>
    public string? CustomerCode { get; set; }

    /// <summary>سقف الائتمان والمديونية المسموح به</summary>
    public decimal CreditLimit { get; set; }

    /// <summary>الرصيد الافتتاحي للعميل</summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>الرصيد قبل بداية الفترة المحددة</summary>
    public decimal BalanceBeforePeriod { get; set; }

    /// <summary>إجمالي الحركات المدينة خلال الفترة (مبيعات ومطالبات)</summary>
    public decimal TotalDebit { get; set; }

    /// <summary>إجمالي الحركات الدائنة خلال الفترة (سدادات ومردودات)</summary>
    public decimal TotalCredit { get; set; }

    /// <summary>الرصيد الحالي النهائي المستحق على العميل</summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>تاريخ بداية فترة التقرير إن تم تحديده</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>تاريخ نهاية فترة التقرير إن تم تحديده</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>قائمة حركات كشف الحساب بالتسلسل الزمني</summary>
    public List<CustomerTransactionResponseDto> Transactions { get; set; } = new();
}
