using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Tenant;

/// <summary>
/// ناقل بيانات التجهيز الشامل لمستأجر جديد (Provision Tenant DTO).
/// يجمع بيانات المنشأة، والفرع الرئيسي الأول، وحساب المستخدم المسؤول (Admin User) لإنشائها كوحدة ذرية واحدة.
/// </summary>
public sealed class ProvisionTenantDto
{
    /// <summary>بيانات المستأجر والمنشأة الأساسية وإعداداتها التشغيلية والمالية</summary>
    public CreateTenantDto Tenant { get; set; } = new();

    /// <summary>اسم الفرع الرئيسي الأول الذي سينشأ تلقائياً للمستأجر</summary>
    [Required, MaxLength(150)]
    public string BranchName { get; set; } = null!;

    /// <summary>عنوان مقر الفرع الرئيسي الأول</summary>
    [MaxLength(500)]
    public string? BranchAddress { get; set; }

    /// <summary>اسم المستخدم لحساب المسؤول الرئيسي للمنشأة</summary>
    [Required, MaxLength(256)]
    public string AdminUserName { get; set; } = null!;

    /// <summary>البريد الإلكتروني للمسؤول الرئيسي</summary>
    [Required, EmailAddress, MaxLength(256)]
    public string AdminEmail { get; set; } = null!;

    /// <summary>كلمة المرور القوية لحساب المسؤول الرئيسي (10 محارف على الأقل)</summary>
    [Required, MinLength(10)]
    public string AdminPassword { get; set; } = null!;
}

/// <summary>
/// ناقل بيانات استجابة التجهيز الناجح للمستأجر والفرع وحساب المسؤول.
/// </summary>
public sealed class ProvisionTenantResponseDto
{
    /// <summary>المعرف الفريد للمستأجر المنشأ</summary>
    public Guid TenantId { get; init; }

    /// <summary>المعرف الفريد للفرع الرئيسي المنشأ</summary>
    public Guid BranchId { get; init; }

    /// <summary>معرف حساب المستخدم المسؤول المنشأ في نظام الهوية</summary>
    public string AdminUserId { get; init; } = null!;
}
