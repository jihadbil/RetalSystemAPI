using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;

namespace RetalSystemAPI.Services.Auth.Implementations;

/// <summary>
/// لقطة بيانات شاملة لصلاحيات المستخدم تتضمن أدواره، وصلاحياته المباشرة والموروثة، والصلاحيات الفعالة.
/// </summary>
/// <param name="Roles">قائمة أسماء الأدوار المسندة للمستخدم</param>
/// <param name="IsCustomized">هل تم تخصيص صلاحيات مستقلة للمستخدم بدلاً من الاكتفاء بالدور</param>
/// <param name="DirectPermissions">الصلاحيات الممنوحة للمستخدم بشكل مباشر</param>
/// <param name="RolePermissions">الصلاحيات الموروثة من كافة أدوار المستخدم</param>
/// <param name="EffectivePermissions">الصلاحيات النهائية الفعالة المعتمدة للتطبيق بعد معالجة القواعد</param>
internal sealed record UserPermissionSnapshot(
    List<string> Roles,
    bool IsCustomized,
    List<string> DirectPermissions,
    List<string> RolePermissions,
    List<string> EffectivePermissions);

/// <summary>
/// فئة مساعدة داخلية لاستخراج وحساب الصلاحيات الفعالة للمستخدم وفق قواعد الصلاحيات والأدوار.
/// </summary>
internal static class UserPermissionResolver
{
    /// <summary>
    /// استخراج وحساب الصلاحيات الفعالة للمستخدم عبر فحص أدواره، ومطالباته المباشرة، وحالة التخصيص.
    /// </summary>
    /// <param name="user">كيان المستخدم المراد فحص صلاحياته</param>
    /// <param name="userManager">مدير مستخدمي الهوية Identity للوصول للأدوار والمطالبات</param>
    /// <param name="roleManager">مدير الأدوار للوصول لصلاحيات كل دور</param>
    /// <returns>لقطة بيانات تحتوي على تفاصيل الصلاحيات بكافة مستوياتها</returns>
    internal static async Task<UserPermissionSnapshot> ResolveAsync(
        ApplicationUser user,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // جلب قائمة أسماء الأدوار المرتبطة بالمستخدم
        var roles = (await userManager.GetRolesAsync(user)).ToList();

        // جلب قائمة المطالبات المسجلة مباشرة على حساب المستخدم
        var claims = await userManager.GetClaimsAsync(user);

        // التحقق مما إذا كان حساب المستخدم يحمل علامة التخصيص المستقل للصلاحيات
        var customized = claims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");

        // استخلاص قيم الصلاحيات المباشرة المسندة للمستخدم من مطالباته
        var direct = PermissionValues(claims);

        // إنشاء مجموعة لتجميع الصلاحيات الموروثة من أدوار المستخدم دون تكرار وبغض النظر عن حالة الأحرف
        var inherited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // المرور على كل دور مسند للمستخدم لجلب الصلاحيات المرتبطة به
        foreach (var name in roles)
        {
            // البحث عن كائن الدور في قاعدة البيانات باستخدام اسمه
            var role = await roleManager.FindByNameAsync(name);

            // في حال وجود الدور، جلب مطالباته واستخلاص صلاحياته وإضافتها لمجموعة الصلاحيات الموروثة
            if (role is not null)
            {
                // إضافة صلاحيات هذا الدور إلى إجمالي الصلاحيات الموروثة
                inherited.UnionWith(PermissionValues(await roleManager.GetClaimsAsync(role)));
            }
        }

        // متغير لتخزين مجموعة الصلاحيات الفعالة النهائية
        IEnumerable<string> effective;

        // إذا كان المستخدم يمتلك دور المسؤول العام للنظام SuperAdmin
        if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
        {
            // يمنح كافة صلاحيات النظام دون أي استثناء
            effective = AppPermissions.GetAllPermissionCodes();
        }
        // إذا كان المستخدم يمتلك دور مدير المستأجر Admin
        else if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
        {
            // يمنح كافة الصلاحيات عدا صلاحية إدارة الأدوار العليا المخصصة للمسؤول العام
            effective = AppPermissions.GetAllPermissionCodes().Where(p => p != Permissions.Roles.Manage);
        }
        // لبقية المستخدمين العاديين
        else
        {
            // إذا كانت الصلاحيات مخصصة له مباشرة يتم اعتماد الصلاحيات المباشرة فقط، وإلا تدمج الموروثة مع المباشرة
            effective = customized ? direct : inherited.Concat(direct);
        }

        // إنشاء وإرجاع كائن لقطة الصلاحيات مع ترتيب القوائم وحذف التكرار
        return new(
            roles,
            customized,
            direct,
            inherited.OrderBy(p => p).ToList(),
            effective.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList());
    }

    /// <summary>
    /// استخراج وتنقية قيم الصلاحيات من مجموعة مطالبات معينة مع حذف التكرار والترتيب الأبجدي.
    /// </summary>
    /// <param name="claims">مجموعة المطالبات المراد فحصها</param>
    /// <returns>قائمة مرتبة بقيم الصلاحيات الفريدة</returns>
    private static List<string> PermissionValues(IEnumerable<Claim> claims)
    {
        // تصفية المطالبات التي تطابق نوع الصلاحيات واستخراج قيمها الفريدة مرتبة
        return claims
            // تصفية المطالبات من نوع الصلاحية مع التأكد من عدم فراغ القيمة
            .Where(c => c.Type == Permissions.ClaimType && !string.IsNullOrWhiteSpace(c.Value))
            // استخراج نص القيمة للصلاحية
            .Select(c => c.Value)
            // إزالة القيم المكررة مع تجاهل حالة الأحرف
            .Distinct(StringComparer.OrdinalIgnoreCase)
            // ترتيب الصلاحيات أبجدياً
            .OrderBy(p => p)
            // تحويل النتيجة إلى قائمة
            .ToList();
    }
}
