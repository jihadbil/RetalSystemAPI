using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Seeders;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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

        // 4. إنشاء أو تحديث المستخدم masoud
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
                var errors = string.Join(", ", result.Errors);
                throw new Exception($"فشل إنشاء المستخدم المستهدف: {errors}");
            }
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
        }
    }
}
