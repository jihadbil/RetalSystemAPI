using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Users;

/// <summary>
/// ناقل بيانات ملخص المستخدم (User Summary DTO).
/// يُستخدم لعرض بيانات المستخدمين الأساسية في الجداول وقوائم الإدارة.
/// </summary>
public class UserSummaryDto
{
    /// <summary>المعرف الفريد للمستخدم</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>اسم الدخول للمستخدم</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>البريد الإلكتروني للمستخدم</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>رقم هاتف المستخدم</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>معرف المستأجر التابع له</summary>
    public Guid TenantId { get; set; }

    /// <summary>معرف الفرع التابع له</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string? BranchName { get; set; }

    /// <summary>قائمة الأدوار المخصصة للمستخدم</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>حالة نشاط الحساب</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ناقل بيانات تفاصيل المستخدم الموسعة (User Details DTO).
/// يتضمن الصلاحيات المباشرة والصلاحيات الفعالة المكتسبة من الأدوار.
/// </summary>
public class UserDetailsDto : UserSummaryDto
{
    /// <summary>الصلاحيات الممنوحة للمستخدم مباشرة</summary>
    public List<string> DirectPermissions { get; set; } = new();

    /// <summary>إجمالي الصلاحيات الفعالة (المكتسبة من الأدوار + المباشرة)</summary>
    public List<string> EffectivePermissions { get; set; } = new();
}

/// <summary>
/// ناقل بيانات إنشاء مستخدم جديد (Create User DTO).
/// </summary>
public class CreateUserDto
{
    /// <summary>اسم الدخول المطلوب</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>البريد الإلكتروني للمستخدم</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>رقم الهاتف</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>كلمة المرور الأولية للحساب</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>معرف المستأجر</summary>
    public Guid TenantId { get; set; }

    /// <summary>معرف الفرع الافتراضي</summary>
    public Guid BranchId { get; set; }

    /// <summary>الدور الأولي المسند للمستخدم</summary>
    public string? Role { get; set; }

    /// <summary>قائمة الصلاحيات الخاصة الإضافية إن وجدت</summary>
    public List<string>? CustomPermissions { get; set; }
}

/// <summary>
/// ناقل بيانات تعديل بيانات مستخدم حالي (Update User DTO).
/// </summary>
public class UpdateUserDto
{
    /// <summary>البريد الإلكتروني للمستخدم</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>رقم الهاتف</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>معرف الفرع الجديد</summary>
    public Guid BranchId { get; set; }

    /// <summary>الدور المسند</summary>
    public string? Role { get; set; }

    /// <summary>حالة تنشيط الحساب</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>قائمة الصلاحيات المخصصة</summary>
    public List<string>? CustomPermissions { get; set; }
}

/// <summary>
/// ناقل بيانات إعادة تعيين كلمة مرور المستخدم (Reset Password DTO).
/// </summary>
public class ResetPasswordDto
{
    /// <summary>كلمة المرور الجديدة</summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// ناقل بيانات الدور وصلاحياته (Role DTO).
/// </summary>
public class RoleDto
{
    /// <summary>معرف الدور</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>اسم الدور (مثل: Admin, Cashier)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>قائمة أكواد الصلاحيات المندرجة تحت الدور</summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// ناقل بيانات الصلاحية المفردة في النظام (Permission DTO).
/// </summary>
public class PermissionDto
{
    /// <summary>كود الصلاحية الفريد (مثل: sales.create)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>اسم الصلاحية المعرب</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>تصنيف أو تبويب الصلاحية (مثل: المبيعات، المخازن)</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>وصف وظيفي للصلاحية</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>هل تمنح هذه الصلاحية إمكانية فتح شاشة معينة</summary>
    public bool IsScreenAccess { get; set; }
}

/// <summary>
/// ناقل بيانات استعراض صلاحيات الدور (Role Permissions DTO).
/// </summary>
public class RolePermissionsDto
{
    /// <summary>معرف الدور</summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>اسم الدور</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>قائمة الصلاحيات المرتبطة بالدور</summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// ناقل بيانات تحديث صلاحيات الدور (Update Role Permissions DTO).
/// </summary>
public class UpdateRolePermissionsDto
{
    /// <summary>معرف الدور المطلوب تعديل صلاحياته</summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>القائمة الجديدة للصلاحيات المسندة للدور</summary>
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// ناقل بيانات استعراض صلاحيات المستخدم التفصيلية (User Permissions DTO).
/// </summary>
public class UserPermissionsDto
{
    /// <summary>معرف المستخدم</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>اسم المستخدم</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>أدوار المستخدم</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>هل لديه صلاحيات مخصصة تختلف عن الأدوار الافتراضية</summary>
    public bool IsCustomized { get; set; }

    /// <summary>الصلاحيات المكتسبة من الأدوار</summary>
    public List<string> RolePermissions { get; set; } = new();

    /// <summary>الصلاحيات الممنوحة مباشرة</summary>
    public List<string> DirectPermissions { get; set; } = new();

    /// <summary>الصلاحيات الفعلية النهائية</summary>
    public List<string> EffectivePermissions { get; set; } = new();
}

/// <summary>
/// ناقل بيانات تحديث الصلاحيات المباشرة للمستخدم (Update User Permissions DTO).
/// </summary>
public class UpdateUserPermissionsDto
{
    /// <summary>معرف المستخدم</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>هل يتم تفعيل وضع الصلاحيات المخصصة</summary>
    public bool IsCustomized { get; set; }

    /// <summary>قائمة الصلاحيات المباشرة الجديدة للمستخدم</summary>
    public List<string> Permissions { get; set; } = new();
}
