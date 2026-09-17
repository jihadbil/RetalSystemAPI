using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// خدمة استخراج TenantId الخاص بالمستأجر الحالي من المطالبات (Claims) في HTTP Context.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// تهيئة الخدمة مع حقن IHttpContextAccessor للوصول إلى معلومات جلسة الطلب.
    /// </summary>
    /// <param name="httpContextAccessor">مزود الوصول إلى سياق الـ HTTP</param>
    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        // حفظ مرجع مزود سياق الطلب لاستخدامه لاحقاً في استخراج المطالبات
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// جلب معرف المستأجر الحالي المفكك من الـ Claims في التوكن (JWT).
    /// </summary>
    public Guid TenantId
    {
        get
        {
            // استخراج كائن المستخدم الرئيسي ClaimsPrincipal من سياق الطلب الحالي
            var user = _httpContextAccessor.HttpContext?.User;
            // إذا لم يكن هناك مستخدم أو سياق طلب، يتم إرجاع Guid.Empty فوراً
            if (user == null) return Guid.Empty;

            // البحث عن مطالبة TenantId بصيغها المختلفة الشائعة (حساسية الأحرف)
            var tenantClaim = user.FindFirst("TenantId")?.Value
                              ?? user.FindFirst("tenantid")?.Value
                              ?? user.FindFirst("tenant_id")?.Value;

            // محاولة تحويل قيمة المطالبة النصية إلى Guid صحيح
            if (Guid.TryParse(tenantClaim, out var tenantId))
            {
                // إرجاع المعرف بعد نجاح التحويل
                return tenantId;
            }

            // في حال عدم وجود المطالبة أو عدم صلاحية تنسيقها يتم إرجاع المعرف الفارغ
            return Guid.Empty;
        }
    }
}
