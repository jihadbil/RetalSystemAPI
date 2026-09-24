using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Models.Customers;

/// <summary>
/// يمثل بيانات العملاء والزبائن في النظام، بما في ذلك التصنيف، السقف الائتماني، والأرصدة الدفترية.
/// </summary>
// كلاس كيان العميل والزبون
public class Customer : TenantBaseEntity
{
    /// <summary>
    /// اسم العميل أو اسم المؤسسة والشركة التجاري.
    /// </summary>
    // خاصية اسم العميل الإلزامي
    public required string Name { get; set; }

    /// <summary>
    /// كود أو رقم العميل التسلسلي التعريفي في النظام.
    /// </summary>
    // خاصية كود العميل التعريفي
    public string? Code { get; set; }

    /// <summary>
    /// البريد الإلكتروني الرسمي للعميل للمراسلات والفواتير الإلكترونية.
    /// </summary>
    // خاصية البريد الإلكتروني للعميل
    public string? Email { get; set; }

    /// <summary>
    /// العنوان الجغرافي أو المقر السكني/التجاري للعميل.
    /// </summary>
    // خاصية عنوان العميل
    public string? Address { get; set; }

    /// <summary>
    /// تصنيف العميل وطبيعة التعامل التجاري معه (قطاعي، جملة، شركات).
    /// </summary>
    // خاصية نوع العميل من تعداد CustomerType
    public CustomerType Type { get; set; } = CustomerType.Retail;

    /// <summary>
    /// الرصيد الافتتاحي المسجل للعميل عند تدشين الحساب في النظام.
    /// </summary>
    // خاصية الرصيد الافتتاحي للعميل
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>
    /// الرصيد الحالي الصافي للعميل (يُحدَّث آلياً مع كل فاتورة وسداد ومرتجع).
    /// </summary>
    // خاصية الرصيد الدفتري الحالي للعميل
    public decimal CurrentBalance { get; set; } = 0;

    /// <summary>
    /// سقف الدين الائتماني المسموح به للعميل في البيع الآجل (0 = غير محدد).
    /// </summary>
    // خاصية الحد الائتماني الأقصى للعميل
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>
    /// حالة نشاط حساب العميل وإمكانية إصدار فواتير ومبيعات له.
    /// </summary>
    // خاصية مؤشر حالة نشاط العميل
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// قائمة أرقام هواتف التواصل المسجلة للعميل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرقام هواتف العميل
    public ICollection<CustomerPhone> CustomerPhones { get; set; } = new List<CustomerPhone>();

    /// <summary>
    /// قائمة فواتير المبيعات الصادرة لهذا العميل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة فواتير مبيعات العميل
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();

    /// <summary>
    /// سجل الحركات المالية ودفتر أستاذ حساب العميل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة الحركات المالية للعميل
    public ICollection<CustomerTransaction> CustomerTransactions { get; set; } = new List<CustomerTransaction>();
}
