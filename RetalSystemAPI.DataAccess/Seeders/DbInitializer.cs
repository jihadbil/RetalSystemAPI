using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;

using System.Security.Claims;
using RetalSystemAPI.Models.Constants;

namespace RetalSystemAPI.DataAccess.Seeders;

/// <summary>
/// فئة استاتيكية مسؤولة عن البذر الأولي للبيانات (Database Seeding) وتطبيق الهجرات تلقائياً عند إقلاع النظام لأول مرة.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// بذر وتأسيس البيانات الأساسية (المستأجر الافتراضي، الفرع الرئيسي، حساب المسؤول الأولي، الأدوار ومصفوفة الصلاحيات).
    /// </summary>
    /// <param name="serviceProvider">مزود الخدمات لحقن سياق البيانات و UserManager</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // إنشاء نطاق خدمة مخصص (Scope) لاستخراج الخدمات المسجلة بنطاق محدد
        using var scope = serviceProvider.CreateScope();
        // استخراج سياق قاعدة البيانات AppDbContext
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // استخراج مدير المستخدمين UserManager
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        // استخراج مدير الأدوار والصلاحيات RoleManager
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // 1. تطبيق جميع الهجرات التلقائية المعلقة (Pending Migrations) في قاعدة البيانات
        await context.Database.MigrateAsync();

        // 2. فحص وجود مستأجر رئيسي افتراضي مع تجاهل مرشحات الاستعلام لضمان الرؤية الشاملة
        var defaultTenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
        // إذا لم يوجد أي مستأجر في النظام بعد
        if (defaultTenant == null)
        {
            // إنشاء كائن المستأجر الرئيسي الافتراضي
            defaultTenant = new Tenant
            {
                // توليد معرف فريد جديد للمستأجر
                Id = Guid.NewGuid(),
                // تعيين الاسم التجاري
                Name = "المستأجر الرئيسي",
                // رقم هاتف التواصل الأساسي
                PhoneNumber = "0912345678",
                // العنوان الجغرافي
                Address = "طرابلس، ليبيا",
                // البريد الإلكتروني للمراسلات
                ContactEmail = "info@retal.com",
                // رابط الشعار (فارغ افتراضياً)
                LogoUrl = "",
                // تفعيل حساب المستأجر
                IsActive = true,
                // تسجيل تاريخ الإنشاء بتوقيت جرينتش
                CreatedAt = DateTime.UtcNow,
                // وسم السجل بأنه غير محذوف
                IsDeleted = false
            };
            // إضافة المستأجر لسياق البيانات
            context.Tenants.Add(defaultTenant);
            // حفظ التغييرات وتثبيت المستأجر في قاعدة البيانات
            await context.SaveChangesAsync();
        }

        // 3. فحص وجود فرع افتراضي تابع للمستأجر الرئيسي مع تجاوز مرشحات الاستعلام
        var defaultBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.TenantId == defaultTenant.Id);
        // إذا لم يوجد فرع رئيسي بعد
        if (defaultBranch == null)
        {
            // إنشاء كائن الفرع الرئيسي الأول
            defaultBranch = new Branch
            {
                // توليد معرف فريد جديد للفرع
                Id = Guid.NewGuid(),
                // تحديد مسمى الفرع
                Name = "الفرع الرئيسي",
                // ربط الفرع بمعرف المستأجر الرئيسي المنشأ أعلاه
                TenantId = defaultTenant.Id,
                // تفعيل حالة الفرع
                IsActive = true,
                // توثيق تاريخ الإنشاء
                CreatedAt = DateTime.UtcNow,
                // تأكيد عدم الحذف
                IsDeleted = false
            };
            // إضافة الفرع لسياق البيانات
            context.Branches.Add(defaultBranch);
            // حفظ وتثبيت الفرع في قاعدة البيانات
            await context.SaveChangesAsync();
        }

        // 4. استدعاء بذر الأدوار الافتراضية ومصفوفة الصلاحيات
        await SeedRolesAndPermissionsAsync(roleManager);

        // 5. إنشاء مسؤول المنصة الأولي هو إجراء اختياري وصريح. لا توجد
        // بيانات اعتماد افتراضية داخل المصدر.
        if (configuration.GetValue<bool>("DatabaseInitialization:SeedInitialAdmin"))
        {
            var adminUserName = configuration["DatabaseInitialization:InitialAdmin:UserName"];
            var adminEmail = configuration["DatabaseInitialization:InitialAdmin:Email"];
            var adminPassword = configuration["DatabaseInitialization:InitialAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminUserName) ||
                string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Initial admin seeding requires UserName, Email and Password in protected configuration.");
            }

            var existingUser = await userManager.FindByNameAsync(adminUserName);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    TenantId = defaultTenant.Id,
                    BranchId = defaultBranch.Id,
                    EmailConfirmed = true,
                    LockoutEnabled = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"فشل إنشاء مسؤول المنصة الأولي: {errors}");
                }

                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }
            else if (!await userManager.IsInRoleAsync(existingUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(existingUser, "SuperAdmin");
            }
        }
    }

    /// <summary>
    /// تأسيس الأدوار الافتراضية للنظام وربط كل دور بحزمة الصلاحيات المناسبة لاختصاصه الوظيفي.
    /// </summary>
    /// <param name="roleManager">مدير إدارة الأدوار RoleManager</param>
    private static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        // مصفوفة بأسماء كافة الأدوار الوظيفية المعتمدة في النظام
        var roles = new[] { "SuperAdmin", "Admin", "Manager", "Cashier", "WarehouseKeeper", "Accountant", "User" };
        // المرور على كل دور والتأكد من وجوده
        foreach (var r in roles)
        {
            // إذا لم يكن الدور مسجلاً من قبل
            if (!await roleManager.RoleExistsAsync(r))
            {
                // إنشاء الدور الجديد وتخزينه في جدول الأدوار
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        if (superAdminRole != null)
        {
            await EnsureRolePermissionsAsync(roleManager, superAdminRole, AppPermissions.GetAllPermissionCodes().ToArray());
        }

        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            var tenantAdminPermissions = AppPermissions.GetAllPermissionCodes()
                .Where(p => p != Permissions.Roles.Manage)
                .ToArray();
            await EnsureRolePermissionsAsync(roleManager, adminRole, tenantAdminPermissions);
        }

        // صلاحيات الكاشير: قراءة وإنشاء المبيعات وفواتيرها ونقاط البيع والعملاء
        var cashierRole = await roleManager.FindByNameAsync("Cashier");
        // التحقق من وجود دور الكاشير
        if (cashierRole != null)
        {
            // حزمة صلاحيات الكاشير
            var cashierPerms = new[]
            {
                Permissions.Pos.View,
                Permissions.Pos.CreateSale,
                Permissions.Pos.ApplyDiscount,
                Permissions.Pos.Reprint,
                Permissions.Pos.OpenSession,
                Permissions.Pos.CloseSession,
                Permissions.SalesInvoices.View,
                Permissions.SalesInvoices.Create,
                Permissions.SalesInvoices.Print,
                Permissions.Customers.View,
                Permissions.Customers.Create,
                Permissions.Products.View
            };
            // تثبيت الصلاحيات على دور الكاشير
            await EnsureRolePermissionsAsync(roleManager, cashierRole, cashierPerms);
        }

        // صلاحيات أمين المخزن: إدارة المخزون والمستودعات والتحويلات والتسويات
        var warehouseRole = await roleManager.FindByNameAsync("WarehouseKeeper");
        // التحقق من وجود دور أمين المخزن
        if (warehouseRole != null)
        {
            // حزمة صلاحيات أمين المخزن
            var whPerms = new[]
            {
                Permissions.Warehouses.View,
                Permissions.Stock.View,
                Permissions.Stock.Adjust,
                Permissions.StockTransfers.View,
                Permissions.StockTransfers.Create,
                Permissions.StockTransfers.Approve,
                Permissions.StockAdjustments.View,
                Permissions.StockAdjustments.Create,
                Permissions.StockAdjustments.Approve,
                Permissions.Products.View,
                Permissions.Categories.View,
                Permissions.Units.View
            };
            // تثبيت الصلاحيات على دور أمين المخزن
            await EnsureRolePermissionsAsync(roleManager, warehouseRole, whPerms);
        }

        // صلاحيات المحاسب: الاطلاع على لوحة المؤشرات، وكافة الفواتير والمردودات والعملاء والموردين
        var accountantRole = await roleManager.FindByNameAsync("Accountant");
        // التحقق من وجود دور المحاسب
        if (accountantRole != null)
        {
            // حزمة صلاحيات المحاسب
            var accPerms = new[]
            {
                Permissions.Dashboard.View,
                Permissions.SalesInvoices.View,
                Permissions.SalesReturns.View,
                Permissions.PurchaseInvoices.View,
                Permissions.PurchaseReturns.View,
                Permissions.PurchaseOrders.View,
                Permissions.Customers.View,
                Permissions.Suppliers.View,
                Permissions.Stock.View,
                Permissions.FinancialReconciliation.View
            };
            // تثبيت الصلاحيات على دور المحاسب
            await EnsureRolePermissionsAsync(roleManager, accountantRole, accPerms);
        }

        // صلاحيات المدير العام: كافة الصلاحيات باستثناء إدارة أدوار النظام وتعديل المستأجر
        var managerRole = await roleManager.FindByNameAsync("Manager");
        // التحقق من وجود دور المدير
        if (managerRole != null)
        {
            // استخراج وتصفية الصلاحيات المناسبة للمدير
            var managerPerms = AppPermissions.GetAllPermissionCodes()
                .Where(p => p != Permissions.Roles.Manage && p != Permissions.Tenants.Edit)
                .ToArray();
            // تثبيت الصلاحيات على دور المدير
            await EnsureRolePermissionsAsync(roleManager, managerRole, managerPerms);
        }
    }

    /// <summary>
    /// التأكد من وجود الصلاحيات المحددة داخل مطالبات الدور (Role Claims) وإضافتها إن لم تكن موجودة.
    /// </summary>
    /// <param name="roleManager">مدير إدارة الأدوار RoleManager</param>
    /// <param name="role">كائن الدور المراد ربط الصلاحيات به</param>
    /// <param name="permissions">مصفوفة أكواد الصلاحيات المطلوب تثبيتها</param>
    private static async Task EnsureRolePermissionsAsync(RoleManager<IdentityRole> roleManager, IdentityRole role, string[] permissions)
    {
        // جلب قائمة المطالبات الحالية المرتبطة بالدور من قاعدة البيانات
        var existingClaims = await roleManager.GetClaimsAsync(role);
        // تصفية المطالبات من نوع الصلاحية وتحويلها إلى HashSet للبحث فائق السرعة O(1)
        var existingPerms = existingClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // المرور على كل صلاحية في المصفوفة المطلوبة
        foreach (var p in permissions)
        {
            // إذا لم تكن الصلاحية مضافة مسبقاً للدور
            if (!existingPerms.Contains(p))
            {
                // إنشاء مطالبة جديدة وإضافتها للدور
                await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p));
            }
        }
    }
}
