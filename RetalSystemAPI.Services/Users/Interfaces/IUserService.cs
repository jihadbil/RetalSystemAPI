using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Users.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مستخدمي النظام والصلاحيات والأدوار وتعيين كلمات المرور وإدارة العزل المتعدد للمستأجرين.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// جلب صفحة بيانات مجزأة من المستخدمين مع إمكانية البحث بالاسم أو البريد أو الهاتف والفلترة بالفرع.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة الحالية</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="search">نص البحث في اسم المستخدم أو البريد أو الهاتف (اختياري)</param>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>صفحة نتائج تحتوي على ملخصات المستخدمين والعدد الإجمالي</returns>
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? branchId,
        CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل مستخدم محدد بالمعرف مع الأدوار والفرع وحالة النشاط والصلاحيات المباشرة والفعالة.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستخدم</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستخدم التفصيلية وصلاحياته</returns>
    Task<ServiceResult<UserDetailsDto>> GetUserByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// إنشاء مستخدم جديد وتعيين دوره وفرعه وكلمة المرور الخاصة به والتحقق من عدم تكرار الاسم والبريد.
    /// </summary>
    /// <param name="dto">بيانات المستخدم الجديد وكلمة المرور والدور</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>ملخص بيانات المستخدم المنشأ حديثاً</returns>
    Task<ServiceResult<UserSummaryDto>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات مستخدم موجود وتعديل دوره وفرعه وحالة القفل/النشاط.
    /// </summary>
    /// <param name="id">معرف المستخدم المراد تحديثه</param>
    /// <param name="dto">البيانات المحدثة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>ملخص بيانات المستخدم بعد التحديث</returns>
    Task<ServiceResult<UserSummaryDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف مستخدم نهائياً من النظام.
    /// </summary>
    /// <param name="id">معرف المستخدم المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteUserAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// إعادة تعيين كلمة مرور المستخدم بكلمة مرور جديدة.
    /// </summary>
    /// <param name="id">معرف المستخدم</param>
    /// <param name="dto">بيانات كلمة المرور الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية إعادة التعيين</returns>
    Task<ServiceResult> ResetPasswordAsync(string id, ResetPasswordDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة الأدوار والصلاحيات المعرفة في النظام مع ضمان تهيئة الأدوار الافتراضية.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بالأدوار وصلاحيات كل دور</returns>
    Task<ServiceResult<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);

    /// <summary>
    /// إنشاء دور جديد في النظام.
    /// </summary>
    /// <param name="roleName">اسم الدور الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الدور المنشأ</returns>
    Task<ServiceResult<RoleDto>> CreateRoleAsync(string roleName, CancellationToken ct = default);

    /// <summary>
    /// حذف دور من النظام مع حظر حذف الأدوار السيادية الأساسية (مثل Admin).
    /// </summary>
    /// <param name="roleId">معرف الدور المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteRoleAsync(string roleId, CancellationToken ct = default);

    /// <summary>
    /// جلب جميع أذونات وصلاحيات النظام المعرفة ومجموعاتها الوظيفية.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بكافة صلاحيات النظام</returns>
    Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صلاحيات دور محدد في النظام.
    /// </summary>
    /// <param name="roleId">معرف الدور</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الدور وقائمة الصلاحيات المسندة له</returns>
    Task<ServiceResult<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);

    /// <summary>
    /// تحديث وصياغة مصفوفة صلاحيات دور محدد.
    /// </summary>
    /// <param name="dto">بيانات الدور وقائمة الصلاحيات الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية التحديث</returns>
    Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب الصلاحيات المباشرة والفعالة لمستخدم محدد (المستمدة من الأدوار والمخصصة).
    /// </summary>
    /// <param name="userId">معرف المستخدم</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>تفاصيل صلاحيات المستخدم المباشرة والموروثة</returns>
    Task<ServiceResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// تحديث الصلاحيات المباشرة لمستخدم محدد وإسناد صلاحيات مخصصة له.
    /// </summary>
    /// <param name="dto">بيانات الصلاحيات المخصصة للمستخدم</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية التحديث</returns>
    Task<ServiceResult> UpdateUserPermissionsAsync(UpdateUserPermissionsDto dto, CancellationToken ct = default);
}
