using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Controllers.Users;

/// <summary>
/// متحكم إدارة المستخدمين وأدوارهم ومصفوفة الصلاحيات (Users, Roles &amp; Permissions) في النظام.
/// يوفر نقاط النهاية لإدارة حسابات المستخدمين، تعيين وإلغاء الأدوار، إعادة تعيين كلمات المرور،
/// وإدارة الصلاحيات التفصيلية على مستوى الأدوار والمستخدمين.
/// </summary>
[Authorize]
[Route("api/users")]
public class UsersController : BaseApiController
{
    /// <summary>
    /// خدمة معالجة وإدارة المستخدمين والأدوار والصلاحيات.
    /// </summary>
    private readonly IUserService _userService;

    /// <summary>
    /// تهيئة نسخة جديدة من متحكم المستخدمين مع حقن خدمة المستخدمين.
    /// </summary>
    /// <param name="userService">واجهة خدمة المستخدمين والأدوار.</param>
    public UsersController(IUserService userService)
    {
        // إسناد خدمة المستخدمين المحقونة إلى الحقل الخاص
        _userService = userService;
    }

    /// <summary>
    /// استرجاع قائمة صفحية بالمستخدمين مع إمكانية البحث النصي والتصفية بحسب الفرع التابع له.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (الافتراضي 1).</param>
    /// <param name="pageSize">عدد السجلات في الصفحة الواحدة (الافتراضي 10).</param>
    /// <param name="search">نص اختياري للبحث في أسماء المستخدمين والبريد وأرقام الهواتف.</param>
    /// <param name="branchId">معرف الفرع لتصفية المستخدمين التابعين له فقط.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة صفحية تحتوي على بيانات ملخص المستخدمين مع تفاصيل الترقيم.</returns>
    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? branchId = null,
        CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لجلب قائمة المستخدمين الصفحية بناءً على معايير البحث والفرع
        var result = await _userService.GetPagedUsersAsync(pageNumber, pageSize, search, branchId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP ملائمة
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع البيانات التفصيلية لمستخدم محدد استناداً إلى معرفه الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستخدم (UserId).</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستخدم التفصيلية شاملاً الأدوار والفروع أو خطأ 404.</returns>
    [HttpGet("{id}")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين للبحث عن تفاصيل المستخدم بالمعرف المحدد
        var result = await _userService.GetUserByIdAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستخدم جديد في النظام وربطه بالفرع والأدوار المحددة وتشفير كلمة مروره.
    /// </summary>
    /// <param name="dto">بيانات المستخدم الجديد بما في ذلك كلمة المرور، البريد، والأدوار.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستخدم المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لتنفيذ إنشاء المستخدم والتحقق من قواعد الأمان وعدم تكرار البيانات
        var result = await _userService.CreateUserAsync(dto, ct);

        // التحقق مما إذا كانت عملية إنشاء المستخدم قد تمت بنجاح
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مصحوباً ببيانات المستخدم ورسالة التأكيد
            return StatusCode(201, ApiResponse<UserSummaryDto>.Ok(result.Data!, "تم إنشاء حساب المستخدم بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل وتحديث بيانات مستخدم قائم (مثل الاسم، البريد، الفرع، وحالة النشاط).
    /// </summary>
    /// <param name="id">المعرف الفريد للمستخدم المراد تعديله.</param>
    /// <param name="dto">البيانات المحدثة للمستخدم.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات المستخدم بعد التعديل أو كود الخطأ المناسب.</returns>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Users.Edit)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لتحديث بيانات المستخدم المحدد بالمعرف
        var result = await _userService.UpdateUserAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستخدم من النظام نهائياً أو تعطيله في حال وجود قيود أمان تمنع حذفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستخدم المطلوب حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح العملية أو توضح مانع الحذف.</returns>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لتنفيذ عملية الحذف والتحقق من قيود الربط
        var result = await _userService.DeleteUserAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إعادة تعيين كلمة المرور لمستخدم محدد من قبل المدير أو المسؤولين المخولين.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستخدم المستهدف.</param>
    /// <param name="dto">بيانات كلمة المرور الجديدة وتأكيدها.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تعيين كلمة المرور أو رسالة الخطأ.</returns>
    [HttpPost("{id}/reset-password")]
    [HasPermission(Permissions.Users.ResetPassword)]
    public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromBody] ResetPasswordDto dto, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لتغيير كلمة المرور وتطبيق سياسات التعقيد
        var result = await _userService.ResetPasswordAsync(id, dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع قائمة بجميع الأدوار (Roles) المعرفة والمتاحة في النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالأدوار المسجلة في النظام.</returns>
    [HttpGet("roles")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لجلب قائمة كافة الأدوار
        var result = await _userService.GetRolesAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء دور أمني جديد في النظام لإسناد الصلاحيات إليه لاحقاً.
    /// </summary>
    /// <param name="request">طلب إنشاء الدور متضمناً اسم الدور الجديد.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>بيانات الدور المنشأ مصحوبة بكود 201 Created عند النجاح.</returns>
    [HttpPost("roles")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لإنشاء الدور الجديد والتحقق من عدم تكراره
        var result = await _userService.CreateRoleAsync(request.Name, ct);

        // التحقق من نجاح عملية إنشاء الدور
        if (result.IsSuccess)
        {
            // إرجاع كود الاستجابة القياسي 201 Created مصحوباً ببيانات الدور ورسالة التأكيد
            return StatusCode(201, ApiResponse<RoleDto>.Ok(result.Data!, "تم إنشاء الدور بنجاح"));
        }

        // تحويل كود ورسالة الخطأ إلى استجابة HTTP ملائمة في حال الفشل
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف دور أمني من النظام في حال عدم ارتباطه بمستخدمين نشطين.
    /// </summary>
    /// <param name="id">المعرف الفريد للدور المطلوب حذفه.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حذف الدور أو توضح مانع الحذف.</returns>
    [HttpDelete("roles/{id}")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> DeleteRole([FromRoute] string id, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لحذف الدور والتحقق من عدم ارتباطه بمستخدمين
        var result = await _userService.DeleteRoleAsync(id, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع الدليل الشامل لجميع أذونات وصلاحيات الشاشات والعمليات المعرفة في النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة شجرية بجميع الصلاحيات والأقسام الوظيفية المتاحة في النظام.</returns>
    [HttpGet("permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لجلب قائمة الصلاحيات الشاملة المعرفة في النظام
        var result = await _userService.GetAllPermissionsAsync(ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع مصفوفة الصلاحيات الممنوحة حالياً لدور وظيفي محدد.
    /// </summary>
    /// <param name="roleId">المعرف الفريد للدور الوظيفي المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بأسماء الصلاحيات المخصصة لهذا الدور.</returns>
    [HttpGet("roles/{roleId}/permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRolePermissions([FromRoute] string roleId, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لجلب الصلاحيات المسندة للدور المحدد
        var result = await _userService.GetRolePermissionsAsync(roleId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث وحفظ مصفوفة الصلاحيات المسندة لدور وظيفي محدد.
    /// </summary>
    /// <param name="roleId">المعرف الفريد للدور المراد تحديث صلاحياته.</param>
    /// <param name="dto">قائمة الصلاحيات الجديدة المراد ربطها بالدور.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح حفظ الصلاحيات للدور.</returns>
    [HttpPut("roles/{roleId}/permissions")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> UpdateRolePermissions([FromRoute] string roleId, [FromBody] UpdateRolePermissionsDto dto, CancellationToken ct = default)
    {
        // تعيين معرف الدور الوارد من المسار في كائن البيانات المنقولة
        dto.RoleId = roleId;

        // استدعاء خدمة المستخدمين لتحديث مصفوفة أذونات الدور
        var result = await _userService.UpdateRolePermissionsAsync(dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// استرجاع الصلاحيات المباشرة والفعالة الممنوحة لمستخدم محدد بشكل فردي.
    /// </summary>
    /// <param name="userId">المعرف الفريد للمستخدم المستهدف.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>قائمة بالصلاحيات المباشرة الخاصة بالمستخدم.</returns>
    [HttpGet("{userId}/permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetUserPermissions([FromRoute] string userId, CancellationToken ct = default)
    {
        // استدعاء خدمة المستخدمين لجلب الصلاحيات المباشرة المسندة للمستخدم
        var result = await _userService.GetUserPermissionsAsync(userId, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث وحفظ الصلاحيات المباشرة الممنوحة لمستخدم محدد لتجاوز أو تخصيص صلاحيات أدواره.
    /// </summary>
    /// <param name="userId">المعرف الفريد للمستخدم المراد تعديل صلاحياته.</param>
    /// <param name="dto">بيانات الصلاحيات المباشرة الجديدة للمستخدم.</param>
    /// <param name="ct">رمز إلغاء العملية لمراقبة مقاطعة الطلب.</param>
    /// <returns>استجابة موحدة تؤكد نجاح تحديث صلاحيات المستخدم.</returns>
    [HttpPut("{userId}/permissions")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> UpdateUserPermissions([FromRoute] string userId, [FromBody] UpdateUserPermissionsDto dto, CancellationToken ct = default)
    {
        // تعيين معرف المستخدم الوارد من المسار في كائن البيانات المنقولة
        dto.UserId = userId;

        // استدعاء خدمة المستخدمين لتحديث مصفوفة الصلاحيات المباشرة للمستخدم
        var result = await _userService.UpdateUserPermissionsAsync(dto, ct);

        // تحويل وإرجاع النتيجة كاستجابة HTTP
        return ToActionResult(result);
    }
}

/// <summary>
/// كائن طلب إنشاء دور أمني جديد في النظام.
/// </summary>
public class CreateRoleRequest
{
    /// <summary>
    /// اسم الدور الأمني المراد إنشاؤه (مثل: Accountant, Cashier).
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
