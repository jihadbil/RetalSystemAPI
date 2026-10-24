using System.Data.Common;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using RetalSystemAPI.Controllers.Auth;
using RetalSystemAPI.Controllers.Branches;
using RetalSystemAPI.Controllers.Tenants;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Extensions;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Auth.Interfaces;
using RetalSystemAPI.Services.Branch.Implementations;
using RetalSystemAPI.Services.Branch.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Extensions;
using RetalSystemAPI.Services.FileUpload.Implementations;
using RetalSystemAPI.Services.Tenant.Implementations;
using RetalSystemAPI.Services.Tenant.Interfaces;
using RetalSystemAPI.Services.Users.Interfaces;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class TenantAuthBranchFixTests
{
    private const string Password = "Retal-Regression-2026!";

    [Fact]
    public void Provisioning_endpoint_requires_platform_admin()
    {
        var method = typeof(TenantsController).GetMethod(nameof(TenantsController.Provision))!;
        Assert.Contains(method.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>(),
            a => a.Roles == "SuperAdmin");
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
    }

    [SqlServerFact]
    public async Task Branch_deletion_preserves_stocked_warehouses_and_financial_history()
    {
        await using var f = await Fixture.CreateAsync();
        var warehouse = new Warehouse { Name = "Stock", BranchId = f.BranchId, Type = WarehouseType.Show };
        f.Db.Add(new ShowroomStock
        {
            Warehouse = warehouse, Quantity = 10,
            Product = new Product { Name = "Stocked item", Category = new Category { Name = "Category" } }
        });
        await f.Db.SaveChangesAsync();
        var controller = new BranchesController(f.Get<IBranchService>());
        Assert.Equal(422, Assert.IsType<ObjectResult>(await controller.Delete(f.BranchId, default)).StatusCode);
        Assert.NotNull(await f.Get<IUnitOfWork>().Warehouses.GetDetailsAsync(warehouse.Id));
        Assert.Equal(10, await f.Db.ShowroomStocks.SumAsync(s => s.Quantity));

        var historical = new Branch { Name = "Historical" };
        f.Db.Add(new Safe { Name = "Archived safe", Branch = historical, Type = SafeType.CashBox, IsDeleted = true });
        await f.Db.SaveChangesAsync();
        var result = await f.Get<IBranchService>().DeleteAsync(historical.Id);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BranchHasDependencies, result.ErrorCode);
        Assert.False((await f.Db.Branches.AsNoTracking().SingleAsync(b => b.Id == historical.Id)).IsDeleted);
    }

    [SqlServerFact]
    public async Task Deleted_branch_releases_phones_and_name_atomically()
    {
        await using var f = await Fixture.CreateAsync();
        var service = f.Get<IBranchService>();
        var created = await service.CreateAsync(new() { Name = "Reusable", Phones = [new() { PhoneNumber = "0912345678" }] });
        Assert.True(created.IsSuccess, created.ErrorMessage);
        Assert.True((await service.DeleteAsync(created.Data!.Id)).IsSuccess);
        Assert.True((await f.Db.BranchPhones.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted);
        var replacement = await service.CreateAsync(new() { Name = "Reusable", Phones = [new() { PhoneNumber = "0912345678" }] });
        Assert.True(replacement.IsSuccess, replacement.ErrorMessage);
        Assert.Single(await f.Db.BranchPhones.AsNoTracking().ToListAsync());
    }

    [SqlServerFact]
    public async Task Phone_update_preserves_unchanged_ids_and_reports_duplicate_input_and_foreign_numbers()
    {
        await using var f = await Fixture.CreateAsync();
        var service = f.Get<IBranchService>();
        var created = (await service.CreateAsync(new() { Name = "Phones", Phones = [new() { PhoneNumber = "0912345678" }] })).Data!;
        var phoneId = created.Phones.Single().Id;
        var updated = await service.UpdateAsync(created.Id, new()
        {
            Id = created.Id, Name = "Phones", IsActive = true,
            Phones = [new() { PhoneNumber = "0912345678", Name = "Updated" }]
        });
        Assert.True(updated.IsSuccess, updated.ErrorMessage);
        Assert.Equal(phoneId, updated.Data!.Phones.Single().Id);
        Assert.Equal("Updated", updated.Data.Phones.Single().Name);
        Assert.True((await service.UpdateAsync(created.Id, new() { Id = created.Id, Name = "Phones", IsActive = true })).IsSuccess);
        var restored = await service.UpdateAsync(created.Id, new()
        { Id = created.Id, Name = "Phones", IsActive = true, Phones = [new() { PhoneNumber = "0912345678" }] });
        Assert.True(restored.IsSuccess, restored.ErrorMessage);
        Assert.NotEqual(phoneId, restored.Data!.Phones.Single().Id);
        var controller = new BranchesController(service);
        Assert.Equal(409, Assert.IsType<ObjectResult>(await controller.Create(new()
        { Name = "Other", Phones = [new() { PhoneNumber = "0912345678" }] }, default)).StatusCode);
        Assert.Equal(409, Assert.IsType<ObjectResult>(await controller.Create(new()
        { Name = "Repeated", Phones = [new() { PhoneNumber = "0922345678" }, new() { PhoneNumber = "0922345678" }] }, default)).StatusCode);
    }

    [SqlServerFact]
    public async Task Concurrent_branch_names_and_phone_numbers_have_one_winner_and_a_conflict_result()
    {
        await using var f = await Fixture.CreateAsync();
        foreach (var phoneRace in new[] { false, true })
        {
            var barrier = new SaveBarrier();
            await using var first = f.Database.Context(f.TenantId, barrier);
            await using var second = f.Database.Context(f.TenantId, barrier);
            var one = new BranchService(new UnitOfWork(first), f.Get<IMapper>()).CreateAsync(new()
            { Name = phoneRace ? "Phone A" : "Concurrent", Phones = phoneRace ? [new() { PhoneNumber = "0912345678" }] : [] });
            var two = new BranchService(new UnitOfWork(second), f.Get<IMapper>()).CreateAsync(new()
            { Name = phoneRace ? "Phone B" : "Concurrent", Phones = phoneRace ? [new() { PhoneNumber = "0912345678" }] : [] });
            var results = await Task.WhenAll(one, two);
            Assert.Single(results, r => r.IsSuccess);
            Assert.Equal(phoneRace ? ErrorCodes.BranchPhoneExists : ErrorCodes.BranchNameExists,
                Assert.Single(results, r => !r.IsSuccess).ErrorCode);
        }
    }

    [SqlServerFact]
    public async Task Database_rejects_dependencies_added_after_branch_was_deleted_and_direct_deletion_with_dependencies()
    {
        await using var f = await Fixture.CreateAsync();
        var empty = new Branch { Name = "Empty" };
        f.Db.Add(empty);
        await f.Db.SaveChangesAsync();
        var id = empty.Id;
        Assert.True((await f.Get<IBranchService>().DeleteAsync(id)).IsSuccess);
        await using var stale = f.Database.Context(f.TenantId);
        stale.Warehouses.Add(new() { Name = "Late warehouse", BranchId = id, Type = WarehouseType.Storge });
        var rejected = await Assert.ThrowsAsync<DbUpdateException>(() => stale.SaveChangesAsync());
        Assert.Equal(51002, Assert.IsType<SqlException>(rejected.InnerException).Number);
        f.Db.Warehouses.Add(new() { Name = "Current", BranchId = f.BranchId, Type = WarehouseType.Storge });
        await f.Db.SaveChangesAsync();
        var guarded = await Assert.ThrowsAsync<SqlException>(() => f.Db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Branches SET IsDeleted = 1 WHERE Id = {f.BranchId}"));
        Assert.Equal(51001, guarded.Number);
    }

    [SqlServerFact]
    public async Task Dependency_inserted_after_service_checks_rolls_back_branch_and_phone_deletion()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.BranchPhones.Add(new() { BranchId = f.BranchId, PhoneNumber = "0912345678" });
        await f.Db.SaveChangesAsync();
        var pause = new PauseDeletion();
        await using var deleting = f.Database.Context(f.TenantId, pause);
        var pending = new BranchService(new UnitOfWork(deleting), f.Get<IMapper>()).DeleteAsync(f.BranchId);
        try
        {
            await pause.Entered.Task.WaitAsync(TimeSpan.FromSeconds(30));
            await using var adding = f.Database.Context(f.TenantId);
            adding.Warehouses.Add(new() { Name = "Concurrent warehouse", BranchId = f.BranchId, Type = WarehouseType.Storge });
            await adding.SaveChangesAsync();
        }
        finally { pause.Resume.TrySetResult(); }
        var result = await pending;
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BranchHasDependencies, result.ErrorCode);
        Assert.False((await f.Db.Branches.AsNoTracking().SingleAsync()).IsDeleted);
        Assert.False((await f.Db.BranchPhones.AsNoTracking().SingleAsync()).IsDeleted);
    }

    [SqlServerFact]
    public async Task Registration_and_user_updates_reject_cross_field_login_collisions_without_breaking_email_login()
    {
        await using var f = await Fixture.CreateAsync();
        var users = f.Get<UserManager<ApplicationUser>>();
        var original = new ApplicationUser { UserName = "reserved@example.test", Email = "owner@example.test", TenantId = f.TenantId, BranchId = f.BranchId };
        Assert.True((await users.CreateAsync(original, Password)).Succeeded);
        var auth = f.Get<IAuthService>();
        var controller = new AuthController(auth);
        var collision = await controller.Register(new()
        { UserName = "OWNER@EXAMPLE.TEST", Email = "other@example.test", Password = Password, BranchId = f.BranchId }, default);
        Assert.Equal(409, Assert.IsType<ObjectResult>(collision).StatusCode);
        var reverse = await auth.RegisterAsync(new()
        { UserName = "other", Email = "reserved@example.test", Password = Password, BranchId = f.BranchId });
        Assert.Equal(ErrorCodes.UserAlreadyExists, reverse.ErrorCode);
        Assert.True((await auth.LoginAsync(new() { UserName = "owner@example.test", Password = Password })).IsSuccess);

        var other = new ApplicationUser { UserName = "valid", Email = "valid@example.test", TenantId = f.TenantId, BranchId = f.BranchId };
        Assert.True((await users.CreateAsync(other, Password)).Succeeded);
        var changed = await f.Get<IUserService>().UpdateUserAsync(other.Id, new()
        { Email = "reserved@example.test", BranchId = f.BranchId, IsActive = true });
        Assert.False(changed.IsSuccess);
        Assert.Equal(ErrorCodes.UserAlreadyExists, changed.ErrorCode);
        Assert.Equal("valid@example.test", (await f.Db.Users.AsNoTracking().SingleAsync(u => u.Id == other.Id)).Email);
    }

    [SqlServerFact]
    public async Task Login_reservations_handle_concurrent_cross_field_claims_and_release_changed_and_deleted_aliases()
    {
        await using var f = await Fixture.CreateAsync();
        var barrier = new SaveBarrier();
        await using var first = f.Database.Context(f.TenantId, barrier);
        await using var second = f.Database.Context(f.TenantId, barrier);
        await using var p1 = IdentityProvider(first);
        await using var p2 = IdentityProvider(second);
        var one = p1.GetRequiredService<UserManager<ApplicationUser>>().CreateAsync(new()
        { UserName = "shared@example.test", Email = "first@example.test", TenantId = f.TenantId, BranchId = f.BranchId }, Password);
        var two = p2.GetRequiredService<UserManager<ApplicationUser>>().CreateAsync(new()
        { UserName = "second", Email = "SHARED@example.test", TenantId = f.TenantId, BranchId = f.BranchId }, Password);
        var results = await Task.WhenAll(one, two);
        Assert.Single(results, r => r.Succeeded);
        Assert.True(LoginIdentifierValidator.IsDuplicate(Assert.Single(results, r => !r.Succeeded)));

        var manager = f.Get<UserManager<ApplicationUser>>();
        var same = new ApplicationUser { UserName = "same@example.test", Email = "same@example.test", TenantId = f.TenantId, BranchId = f.BranchId };
        Assert.True((await manager.CreateAsync(same, Password)).Succeeded);
        Assert.Equal(1, await f.Db.UserLoginIdentifiers.CountAsync(x => x.UserId == same.Id));
        same.Email = "changed@example.test";
        Assert.True((await manager.UpdateAsync(same)).Succeeded);
        Assert.Equal(2, await f.Db.UserLoginIdentifiers.CountAsync(x => x.UserId == same.Id));
        Assert.True((await manager.DeleteAsync(same)).Succeeded);
        Assert.False(await f.Db.UserLoginIdentifiers.AnyAsync(x => x.UserId == same.Id));
    }

    [SqlServerFact]
    public async Task Provisioned_admin_can_login_to_new_tenant_and_caller_scope_is_unchanged()
    {
        await using var f = await Fixture.CreateAsync();
        var result = await f.Get<ITenantProvisioningService>().ProvisionAsync(ProvisionRequest());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var data = result.Data!;
        await using var target = f.Database.Context(data.TenantId);
        Assert.Equal(data.BranchId, (await target.Branches.SingleAsync()).Id);
        Assert.Equal(data.TenantId, (await target.Users.SingleAsync()).TenantId);
        var login = await f.Get<IAuthService>().LoginAsync(new() { UserName = "new-admin", Password = Password });
        Assert.True(login.IsSuccess, login.ErrorMessage);
        Assert.Equal(data.TenantId, login.Data!.TenantId);
        Assert.Contains("Admin", login.Data.Roles);
        Assert.Equal(f.TenantId, f.Db.CurrentTenantId);
        Assert.Equal(f.TenantId.ToString(), f.Http.HttpContext!.User.FindFirstValue("TenantId"));
    }

    [SqlServerFact]
    public async Task Provisioning_denies_tenant_admins_and_rolls_back_identity_validation_failures()
    {
        await using var f = await Fixture.CreateAsync();
        var request = ProvisionRequest();
        request.AdminPassword = "abcdefghij";
        Assert.False((await f.Get<ITenantProvisioningService>().ProvisionAsync(request)).IsSuccess);
        Assert.False(await f.Db.Tenants.AnyAsync(t => t.Name == request.Tenant.Name));
        Assert.False(await f.Db.Branches.IgnoreQueryFilters().AnyAsync(b => b.Name == request.BranchName));
        f.Http.HttpContext!.User = Principal(f.TenantId, "Admin");
        var denied = await f.Get<ITenantProvisioningService>().ProvisionAsync(ProvisionRequest());
        Assert.Equal(ErrorCodes.Forbidden, denied.ErrorCode);
        Assert.Throws<UnauthorizedAccessException>(() => f.Get<ITenantExecutionScopeFactory>().CreateForProvisioning(Guid.NewGuid()));
    }

    [SqlServerFact]
    public async Task Provisioning_rolls_back_user_aliases_tenant_and_branch_if_role_assignment_fails()
    {
        var fail = new FailRoleAssignment();
        await using var f = await Fixture.CreateAsync(fail);
        fail.Armed = true;
        var request = ProvisionRequest();
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Get<ITenantProvisioningService>().ProvisionAsync(request));
        Assert.False(await f.Db.Tenants.AnyAsync(t => t.Name == request.Tenant.Name));
        Assert.False(await f.Db.Users.AnyAsync(u => u.UserName == request.AdminUserName));
        Assert.False(await f.Db.UserLoginIdentifiers.AnyAsync(x => x.NormalizedValue == "NEW-ADMIN"));
        Assert.False(await f.Db.Branches.IgnoreQueryFilters().AnyAsync(b => b.Name == request.BranchName));
    }

    [SqlServerFact]
    public async Task Logo_response_queries_precede_commit_and_failed_queries_preserve_previous_logo()
    {
        var state = new LogoQueries();
        await using var f = await Fixture.CreateAsync(state, state.Commits);
        var root = Directory.CreateTempSubdirectory("retal-logo-fix-").FullName;
        try
        {
            var files = new FileUploadService(new TestEnvironment(root), f.Get<ICurrentTenantService>(), NullLogger<FileUploadService>.Instance);
            var controller = new TenantsController(f.Get<ITenantService>(), files)
            { ControllerContext = new ControllerContext { HttpContext = f.Http.HttpContext! } };
            using var png = new MemoryStream([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);
            var response = await controller.UploadLogo(new FormFile(png, 0, png.Length, "file", "logo.png"), default);
            Assert.Equal(200, Assert.IsType<OkObjectResult>(response).StatusCode);
            var persisted = (await f.Db.Tenants.AsNoTracking().SingleAsync(t => t.Id == f.TenantId)).LogoUrl;
            Assert.True(File.Exists(Path.Combine(root, persisted.TrimStart('/'))));
            state.FailPendingLogo = true;
            state.Commits.Committed = false;
            png.Position = 0;
            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.UploadLogo(new FormFile(png, 0, png.Length, "file", "new.png"), default));
            Assert.Equal(persisted, (await f.Db.Tenants.AsNoTracking().SingleAsync(t => t.Id == f.TenantId)).LogoUrl);
            Assert.Single(Directory.GetFiles(root, "*.png", SearchOption.AllDirectories));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [SqlServerFact]
    public async Task Migration_refuses_duplicate_branch_names_before_changing_schema()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync("20261016121121_FixDataAccessConsistency");
        var tenant = Guid.NewGuid();
        await using var db = database.Context(tenant);
        db.AddRange(new Tenant { Id = tenant, Name = "Legacy", PhoneNumber = "0912345678" },
            new Branch { Name = "Duplicate" }, new Branch { Name = "Duplicate" });
        await db.SaveChangesAsync();
        var failure = await Assert.ThrowsAsync<SqlException>(() => db.GetService<IMigrator>().MigrateAsync());
        Assert.Equal(51000, failure.Number);
        Assert.Contains("branch names", failure.Message);
        Assert.DoesNotContain(await db.Database.GetAppliedMigrationsAsync(), m => m.EndsWith("HardenTenantAuthAndBranchLifecycle"));
        Assert.Equal(2, await db.Branches.CountAsync());
    }

    [SqlServerFact]
    public async Task Migration_refuses_legacy_login_collisions_and_backfills_fixed_aliases_and_deleted_phones()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync("20261016121121_FixDataAccessConsistency");
        var tenant = Guid.NewGuid();
        await using var db = database.Context(tenant);
        var branch = new Branch { Name = "Legacy branch" };
        var first = new ApplicationUser { UserName = "owner", NormalizedUserName = "OWNER", Email = "shared@example.test", NormalizedEmail = "SHARED@EXAMPLE.TEST", TenantId = tenant, Branch = branch };
        var collision = new ApplicationUser { UserName = "shared@example.test", NormalizedUserName = "SHARED@EXAMPLE.TEST", Email = "other@example.test", NormalizedEmail = "OTHER@EXAMPLE.TEST", TenantId = tenant, Branch = branch };
        db.AddRange(new Tenant { Id = tenant, Name = "Legacy", PhoneNumber = "0912345678" }, first, collision,
            new BranchPhone { Branch = branch, PhoneNumber = "0912345678" });
        await db.SaveChangesAsync();
        var failure = await Assert.ThrowsAsync<SqlException>(() => db.GetService<IMigrator>().MigrateAsync());
        Assert.Equal(51000, failure.Number);
        Assert.Contains("login collisions", failure.Message);
        collision.UserName = "fixed";
        collision.NormalizedUserName = "FIXED";
        // Legacy soft-deletion left phones live; migration safely retires just those phones.
        branch.IsDeleted = true;
        await db.SaveChangesAsync();
        await db.GetService<IMigrator>().MigrateAsync();
        Assert.Equal(4, await db.UserLoginIdentifiers.CountAsync());
        Assert.True((await db.BranchPhones.IgnoreQueryFilters().AsNoTracking().SingleAsync()).IsDeleted);
    }

    [SqlServerFact]
    public async Task Idempotent_upgrade_script_can_be_applied_twice()
    {
        const string previous = "20261016121121_FixDataAccessConsistency";
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync(previous);
        await using var db = database.Context();
        var script = db.GetService<IMigrator>().GenerateScript(previous, options: MigrationsSqlGenerationOptions.Idempotent);
        var batches = System.Text.RegularExpressions.Regex.Split(script, @"^\s*GO\s*$",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        await db.Database.OpenConnectionAsync();
        for (var run = 0; run < 2; run++)
            foreach (var batch in batches.Where(b => !string.IsNullOrWhiteSpace(b)))
                await db.Database.ExecuteSqlRawAsync(batch);
        Assert.Contains(await db.Database.GetAppliedMigrationsAsync(), m => m.EndsWith("HardenTenantAuthAndBranchLifecycle"));
        Assert.Equal(0, await db.UserLoginIdentifiers.CountAsync());
    }

    private static ProvisionTenantDto ProvisionRequest() => new()
    {
        Tenant = new() { Name = "New tenant", PhoneNumber = "0912345678" }, BranchName = "New tenant main",
        AdminUserName = "new-admin", AdminEmail = "new-admin@example.test", AdminPassword = Password
    };

    private static ClaimsPrincipal Principal(Guid tenant, string role = "SuperAdmin") => new(new ClaimsIdentity([
        new(ClaimTypes.NameIdentifier, "platform-operator"), new(ClaimTypes.Role, role), new("TenantId", tenant.ToString())
    ], "test"));

    private static ServiceProvider IdentityProvider(AppDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
            .AddUserStore<LoginIdentifierUserStore>().AddUserValidator<LoginIdentifierValidator>();
        return services.BuildServiceProvider();
    }

    private sealed class SaveBarrier : SaveChangesInterceptor
    {
        private int arrivals;
        private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
            InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (Interlocked.Increment(ref arrivals) == 2) ready.TrySetResult();
            await ready.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            return result;
        }
    }

    private sealed class PauseDeletion : SaveChangesInterceptor
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Resume { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
            InterceptionResult<int> result, CancellationToken ct = default)
        {
            Entered.TrySetResult();
            await Resume.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);
            return result;
        }
    }

    private sealed class FailRoleAssignment : SaveChangesInterceptor
    {
        public bool Armed { get; set; }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data,
            InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (Armed && data.Context!.ChangeTracker.Entries<IdentityUserRole<string>>().Any(e => e.State == EntityState.Added))
                throw new InvalidOperationException("Injected role assignment failure");
            return ValueTask.FromResult(result);
        }
    }

    private sealed class LogoCommit : SaveChangesInterceptor
    {
        public bool Committed { get; set; }
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData data, int result, CancellationToken ct = default)
        {
            if (data.Context!.ChangeTracker.Entries<Tenant>().Any(e => e.Entity.LogoUrl.StartsWith("/uploads/"))) Committed = true;
            return ValueTask.FromResult(result);
        }
    }

    private sealed class LogoQueries : DbCommandInterceptor
    {
        public LogoCommit Commits { get; } = new();
        public bool FailPendingLogo { get; set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData data, InterceptionResult<DbDataReader> result, CancellationToken ct = default)
        {
            if (command.CommandText.Contains("GROUP BY") && command.CommandText.Contains("[Branches]"))
            {
                if (Commits.Committed) throw new InvalidOperationException("A response query ran after the logo committed");
                if (FailPendingLogo && data.Context!.ChangeTracker.Entries<Tenant>().Any(e => e.Property(t => t.LogoUrl).IsModified))
                    throw new InvalidOperationException("Injected statistics read failure");
            }
            return ValueTask.FromResult(result);
        }
    }

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "test";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Test";
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public ModelIntegritySqlServerTests.TestDatabase Database { get; private init; } = null!;
        private ServiceProvider Provider { get; init; } = null!;
        private AsyncServiceScope Scope { get; init; }
        public TestHttpAccessor Http { get; init; } = null!;
        public Guid TenantId { get; init; }
        public Guid BranchId { get; init; }
        public AppDbContext Db => Get<AppDbContext>();
        public T Get<T>() where T : notnull => Scope.ServiceProvider.GetRequiredService<T>();
        public static async Task<Fixture> CreateAsync(params IInterceptor[] interceptors)
        {
            var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
            await using var metadata = database.Context();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = metadata.Database.GetConnectionString(),
                ["JwtSettings:Secret"] = "Local-test-only-placeholder-secret-at-least-32-bytes",
                ["JwtSettings:Issuer"] = "test", ["JwtSettings:Audience"] = "test"
            }).Build();
            var tenantId = Guid.NewGuid();
            var http = new TestHttpAccessor { HttpContext = new DefaultHttpContext { User = Principal(tenantId) } };
            var services = new ServiceCollection();
            services.AddLogging(); services.AddDataProtection(); services.AddSingleton<IConfiguration>(config);
            services.AddDataAccess(config); services.AddServicesLayer(config);
            services.AddSingleton<IHttpContextAccessor>(http);
            services.AddDbContext<AppDbContext>((_, options) => options.AddInterceptors(interceptors));
            var provider = services.BuildServiceProvider();
            var f = new Fixture { Database = database, Provider = provider, Scope = provider.CreateAsyncScope(),
                Http = http, TenantId = tenantId, BranchId = Guid.NewGuid() };
            f.Db.AddRange(new Tenant { Id = tenantId, Name = "Caller tenant", PhoneNumber = "0923456789" },
                new Branch { Id = f.BranchId, Name = "Caller branch" });
            await f.Db.SaveChangesAsync();
            Assert.True((await f.Get<RoleManager<IdentityRole>>().CreateAsync(new("Admin"))).Succeeded);
            return f;
        }
        public async ValueTask DisposeAsync()
        {
            await Scope.DisposeAsync(); await Provider.DisposeAsync(); await Database.DisposeAsync();
        }
    }

    private sealed class TestHttpAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
