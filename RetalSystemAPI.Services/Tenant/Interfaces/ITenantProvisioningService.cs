using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Tenant.Interfaces;

/// <summary>
/// واجهة خدمة تهيئة وإعداد المستأجر الجديد وتأسيس بياناته الأساسية وحسابه الإداري وفرعه الأول.
/// </summary>
public interface ITenantProvisioningService
{
    /// <summary>
    /// تهيئة مستأجر جديد في بيئة العمل متعددة المستأجرين وإنشاء الفرع الرئيسي وحساب المسؤول الإداري ضمن معاملة واحدة.
    /// </summary>
    /// <param name="dto">بيانات المستأجر والفرع ومسؤول النظام المراد إنشاؤهم</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>نتيجة الخدمة متضمنة معرف المستأجر والفرع ومعرف حساب المسؤول المنشأ</returns>
    /// <remarks>
    /// تتطلب هذه العملية صلاحيات المسؤول العام للمنصة (SuperAdmin).
    /// </remarks>
    Task<ServiceResult<ProvisionTenantResponseDto>> ProvisionAsync(ProvisionTenantDto dto, CancellationToken ct = default);
}
