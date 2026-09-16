using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. تطبيق جميع الهجرات التلقائية (Migrations)
        await context.Database.MigrateAsync();

        // 2. إنشاء مستأجر افتراضي إن لم يوجد أي مستأجر
        var defaultTenant = await context.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync();
        if (defaultTenant == null)
        {
            defaultTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "المستأجر الرئيسي",
                PhoneNumber = "0912345678",
                Address = "طرابلس، ليبيا",
                ContactEmail = "info@retal.com",
                LogoUrl = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Tenants.Add(defaultTenant);
            await context.SaveChangesAsync();
        }

        // 3. إنشاء فرع افتراضي إن لم يوجد أي فرع
        var defaultBranch = await context.Branches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.TenantId == defaultTenant.Id);
        if (defaultBranch == null)
        {
            defaultBranch = new Branch
            {
                Id = Guid.NewGuid(),
                Name = "الفرع الرئيسي",
                TenantId = defaultTenant.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Branches.Add(defaultBranch);
            await context.SaveChangesAsync();
        }

        // 4. بذر الأدوار الافتراضية والصلاحيات
        await SeedRolesAndPermissionsAsync(roleManager);

        // 5. إنشاء أو تحديث المستخدم masoud
        var existingUser = await userManager.FindByNameAsync("masoud");
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "masoud",
                Email = "masoud@retal.com",
                TenantId = defaultTenant.Id,
                BranchId = defaultBranch.Id,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "Admin@123");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"فشل إنشاء المستخدم المستهدف: {errors}");
            }

            await userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            // التأكد من تحديث TenantId و BranchId في حال كان الحساب موجوداً بقيم فارغة سابقاً
            bool needsUpdate = false;
            if (existingUser.TenantId == Guid.Empty)
            {
                existingUser.TenantId = defaultTenant.Id;
                needsUpdate = true;
            }
            if (existingUser.BranchId == Guid.Empty)
            {
                existingUser.BranchId = defaultBranch.Id;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                await userManager.UpdateAsync(existingUser);
            }

            if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                await userManager.AddToRoleAsync(existingUser, "Admin");
            }
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "Manager", "Cashier", "WarehouseKeeper", "Accountant", "User" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                await roleManager.CreateAsync(new IdentityRole(r));
            }
        }

        // صلاحيات الكاشير
        var cashierRole = await roleManager.FindByNameAsync("Cashier");
        if (cashierRole != null)
        {
            var cashierPerms = new[]
            {
                Permissions.Pos.View,
                Permissions.Pos.CreateSale,
                Permissions.Pos.ApplyDiscount,
                Permissions.Pos.Reprint,
                Permissions.SalesInvoices.View,
                Permissions.SalesInvoices.Create,
                Permissions.SalesInvoices.Print,
                Permissions.Customers.View,
                Permissions.Customers.Create,
                Permissions.Products.View
            };
            await EnsureRolePermissionsAsync(roleManager, cashierRole, cashierPerms);
        }

        // صلاحيات أمين المخزن
        var warehouseRole = await roleManager.FindByNameAsync("WarehouseKeeper");
        if (warehouseRole != null)
        {
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
            await EnsureRolePermissionsAsync(roleManager, warehouseRole, whPerms);
        }

        // صلاحيات المحاسب
        var accountantRole = await roleManager.FindByNameAsync("Accountant");
        if (accountantRole != null)
        {
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
                Permissions.Stock.View
            };
            await EnsureRolePermissionsAsync(roleManager, accountantRole, accPerms);
        }

        // صلاحيات المدير العام
        var managerRole = await roleManager.FindByNameAsync("Manager");
        if (managerRole != null)
        {
            var managerPerms = AppPermissions.GetAllPermissionCodes()
                .Where(p => p != Permissions.Roles.Manage && p != Permissions.Tenants.Edit)
                .ToArray();
            await EnsureRolePermissionsAsync(roleManager, managerRole, managerPerms);
        }
    }

    private static async Task EnsureRolePermissionsAsync(RoleManager<IdentityRole> roleManager, IdentityRole role, string[] permissions)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingPerms = existingClaims
            .Where(c => c.Type == Permissions.ClaimType)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var p in permissions)
        {
            if (!existingPerms.Contains(p))
            {
                await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, p));
            }
        }
    }
}
