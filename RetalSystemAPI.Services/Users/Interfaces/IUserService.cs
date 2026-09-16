using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Users.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مستخدمي النظام والصلاحيات والأدوار وتعيين كلمات المرور.
/// </summary>
public interface IUserService
{
    /// <summary>جلب صفحة بيانات مجزأة من المستخدمين مع إمكانية البحث والفلترة بالفرع</summary>
    Task<ServiceResult<PagedResult<UserSummaryDto>>> GetPagedUsersAsync(
        int pageNumber,
        int pageSize,
        string? search,
        Guid? branchId,
        CancellationToken ct = default);

    /// <summary>جلب تفاصيل مستخدم محدد بالمعرف مع الأدوار والفرع وحالة النشاط</summary>
    Task<ServiceResult<UserDetailsDto>> GetUserByIdAsync(string id, CancellationToken ct = default);

    /// <summary>إنشاء مستخدم جديد وتعيين دوره وفرعه وكلمة المرور الخاصة به</summary>
    Task<ServiceResult<UserSummaryDto>> CreateUserAsync(CreateUserDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات مستخدم موجود وتعديل دوره وفرعه وحالة القفل/النشاط</summary>
    Task<ServiceResult<UserSummaryDto>> UpdateUserAsync(string id, UpdateUserDto dto, CancellationToken ct = default);

    /// <summary>حذف مستخدم نهائياً من النظام</summary>
    Task<ServiceResult> DeleteUserAsync(string id, CancellationToken ct = default);

    /// <summary>إعادة تعيين كلمة مرور المستخدم</summary>
    Task<ServiceResult> ResetPasswordAsync(string id, ResetPasswordDto dto, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة الأدوار والصلاحيات المعرفة في النظام</summary>
    Task<ServiceResult<List<RoleDto>>> GetRolesAsync(CancellationToken ct = default);

    /// <summary>إنشاء دور جديد في النظام</summary>
    Task<ServiceResult<RoleDto>> CreateRoleAsync(string roleName, CancellationToken ct = default);

    /// <summary>حذف دور من النظام</summary>
    Task<ServiceResult> DeleteRoleAsync(string roleId, CancellationToken ct = default);

    /// <summary>جلب جميع أذونات وصلاحيات النظام المعرفة</summary>
    Task<ServiceResult<List<PermissionDto>>> GetAllPermissionsAsync(CancellationToken ct = default);

    /// <summary>جلب صلاحيات دور محدد</summary>
    Task<ServiceResult<RolePermissionsDto>> GetRolePermissionsAsync(string roleId, CancellationToken ct = default);

    /// <summary>تحديث وصياغة مصفوفة صلاحيات دور محدد</summary>
    Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsDto dto, CancellationToken ct = default);

    /// <summary>جلب الصلاحيات المباشرة والفعالة لمستخدم محدد</summary>
    Task<ServiceResult<UserPermissionsDto>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

    /// <summary>تحديث الصلاحيات المباشرة لمستخدم محدد</summary>
    Task<ServiceResult> UpdateUserPermissionsAsync(UpdateUserPermissionsDto dto, CancellationToken ct = default);
}
