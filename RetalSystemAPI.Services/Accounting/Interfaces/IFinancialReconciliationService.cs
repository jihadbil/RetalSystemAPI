using RetalSystemAPI.Models.DTOs.Accounting.Reconciliation;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة التدقيق والمطابقة المالية لفحص اتساق المستندات التجارية مع دفاتر العملاء والموردين والخزائن (قراءة فقط دون تعديل).
/// </summary>
public interface IFinancialReconciliationService
{
    /// <summary>
    /// إجراء تدقيق ومطابقة شاملة لكافة الحركات المالية المكتملة واكتشاف الفجوات المحاسبية بين المستندات ودفاتر الأستاذ.
    /// </summary>
    /// <param name="branchId">معرف الفرع للتدقيق المخصص أو فارغ لكافة فروع المستأجر</param>
    /// <param name="tolerance">نسبة السماح المقبولة في فروقات التقريب المالي (الافتراضي 0.01)</param>
    /// <param name="maxIssues">الحد الأقصى لعدد المشكلات والفجوات المكتشفة في التقرير</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>تقرير شامل يوضح اتساق الحركات وقائمة بالفجوات المكتشفة</returns>
    // تنفيذ عملية تدقيق الحسابات والمطابقة المالية المتقدمة
    Task<ServiceResult<FinancialReconciliationReportDto>> AuditAsync(
        Guid? branchId = null,
        decimal tolerance = 0.01m,
        int maxIssues = 500,
        CancellationToken ct = default);
}
