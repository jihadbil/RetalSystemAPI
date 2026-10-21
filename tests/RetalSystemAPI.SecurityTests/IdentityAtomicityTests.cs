using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Auth.Implementations;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Users.Implementations;
using System.IdentityModel.Tokens.Jwt;
using Xunit;
using Fixture = RetalSystemAPI.SecurityTests.ServiceLayerRegressionTests.Fixture;

namespace RetalSystemAPI.SecurityTests;

public sealed class IdentityAtomicityTests
{
    [SqlServerFact]
    public async Task Successful_user_and_permission_updates_commit_profile_roles_claims_and_session_revocation()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        var user = await host.CreateUserAsync("commit");
        var stamp = user.SecurityStamp;
        var update = await host.Service.UpdateUserAsync(user.Id, new()
        { Email = "updated@example.test", BranchId = f.Branch.Id, Role = "WarehouseKeeper", IsActive = false });
        Assert.True(update.IsSuccess, update.ErrorMessage);
        var permissions = await host.Service.UpdateUserPermissionsAsync(new()
        { UserId = user.Id, IsCustomized = true, Permissions = [Permissions.Stock.View] });
        Assert.True(permissions.IsSuccess, permissions.ErrorMessage);
        await using var check = database.Context(tenant);
        var persisted = await check.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal("updated@example.test", persisted.Email);
        Assert.NotNull(persisted.LockoutEnd); Assert.NotEqual(stamp, persisted.SecurityStamp);
        var roleNames = await check.UserRoles.Where(r => r.UserId == user.Id).Join(check.Roles, r => r.RoleId, r => r.Id, (_, r) => r.Name).ToListAsync();
        Assert.Equal(["WarehouseKeeper"], roleNames);
        var claims = await check.UserClaims.Where(c => c.UserId == user.Id).ToListAsync();
        Assert.Contains(claims, c => c.ClaimType == "Permissions.Customized" && c.ClaimValue == "true");
        Assert.Contains(claims, c => c.ClaimType == Permissions.ClaimType && c.ClaimValue == Permissions.Stock.View);
    }

    [SqlServerFact]
    public async Task Actual_SQL_concurrent_identity_update_does_not_overwrite_the_winning_profile_or_roles()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        var user = await host.CreateUserAsync("concurrent");
        var winningStamp = Guid.NewGuid().ToString();
        await using (var other = database.Context(tenant))
        {
            var winning = await other.Users.SingleAsync(u => u.Id == user.Id);
            winning.Email = "winner@example.test"; winning.ConcurrencyStamp = Guid.NewGuid().ToString(); winning.SecurityStamp = winningStamp;
            await other.SaveChangesAsync();
        }
        var result = await host.Service.UpdateUserAsync(user.Id, new()
        { Email = "loser@example.test", BranchId = f.Branch.Id, Role = "WarehouseKeeper", IsActive = false });
        Assert.False(result.IsSuccess); Assert.Equal(ErrorCodes.ConcurrencyError, result.ErrorCode);
        await using var check = database.Context(tenant);
        var persisted = await check.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal("winner@example.test", persisted.Email); Assert.Equal(winningStamp, persisted.SecurityStamp); Assert.Null(persisted.LockoutEnd);
        var roles = await check.UserRoles.Where(r => r.UserId == user.Id).Join(check.Roles, r => r.RoleId, r => r.Id, (_, r) => r.Name).ToListAsync();
        Assert.Equal(["Cashier"], roles);
    }

    [SqlServerFact]
    public async Task Failed_role_assignment_rolls_back_the_new_user()
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        host.Fault.FailUserUpdateAt = 1;
        var result = await host.Service.CreateUserAsync(new()
        {
            UserName = "new-user", Email = "new@example.test", Password = Password,
            BranchId = f.Branch.Id, Role = "Cashier"
        });
        Assert.False(result.IsSuccess);
        await using var check = database.Context(tenant);
        Assert.False(await check.Users.AnyAsync(u => u.UserName == "new-user"));
    }

    [SqlServerTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Failed_user_update_restores_profile_roles_and_session_stamp(int failedStep)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        var user = await host.CreateUserAsync("original");
        var stamp = user.SecurityStamp;
        host.Fault.FailUserUpdateAt = failedStep;
        var result = await host.Service.UpdateUserAsync(user.Id, new()
        {
            Email = "changed@example.test", BranchId = f.Branch.Id, Role = "WarehouseKeeper", IsActive = false
        });
        Assert.False(result.IsSuccess);
        await using var check = database.Context(tenant);
        var persisted = await check.Users.SingleAsync(u => u.Id == user.Id);
        Assert.Equal("original@example.test", persisted.Email);
        Assert.Null(persisted.LockoutEnd);
        Assert.Equal(stamp, persisted.SecurityStamp);
        var roles = await check.UserRoles.Where(r => r.UserId == user.Id)
            .Join(check.Roles, r => r.RoleId, r => r.Id, (_, r) => r.Name).ToListAsync();
        Assert.Equal(["Cashier"], roles);
    }

    [SqlServerTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Failed_permission_update_restores_all_claims_and_stamp(int failedStep)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        var user = await host.CreateUserAsync("customized");
        Assert.True((await host.Users.AddClaimsAsync(user,
            [new("Permissions.Customized", "true"), new(Permissions.ClaimType, Permissions.Products.View)])).Succeeded);
        var stamp = user.SecurityStamp;
        host.ResetFaultCounters();
        host.Fault.FailUserUpdateAt = failedStep;
        var result = await host.Service.UpdateUserPermissionsAsync(new()
        {
            UserId = user.Id, IsCustomized = true, Permissions = [Permissions.Products.Edit]
        });
        Assert.False(result.IsSuccess);
        await using var check = database.Context(tenant);
        var claims = await check.UserClaims.Where(c => c.UserId == user.Id).ToListAsync();
        Assert.Equal(2, claims.Count);
        Assert.Contains(claims, c => c.ClaimType == "Permissions.Customized" && c.ClaimValue == "true");
        Assert.Contains(claims, c => c.ClaimType == Permissions.ClaimType && c.ClaimValue == Permissions.Products.View);
        Assert.Equal(stamp, await check.Users.Where(u => u.Id == user.Id).Select(u => u.SecurityStamp).SingleAsync());
    }

    [SqlServerTheory]
    [InlineData(1, 0, false)]
    [InlineData(2, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(0, 2, false)]
    [InlineData(0, 2, true)]
    public async Task Failed_role_change_restores_role_claims_memberships_and_all_sessions(int roleStep, int userStep, bool delete)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        await host.CreateUserAsync("one");
        await host.CreateUserAsync("two");
        var stamps = await f.Db.Users.ToDictionaryAsync(u => u.Id, u => u.SecurityStamp);
        var role = (await host.Roles.FindByNameAsync("Cashier"))!;
        host.Fault.FailRoleUpdateAt = roleStep;
        host.Fault.FailUserUpdateAt = userStep;
        var result = delete ? await host.Service.DeleteRoleAsync(role.Id)
            : await host.Service.UpdateRolePermissionsAsync(new() { RoleId = role.Id, Permissions = [Permissions.Products.Edit] });
        Assert.False(result.IsSuccess);
        await using var check = database.Context(tenant);
        Assert.True(await check.Roles.AnyAsync(r => r.Id == role.Id));
        Assert.Equal(2, await check.UserRoles.CountAsync(r => r.RoleId == role.Id));
        Assert.Equal([Permissions.Products.View], await check.RoleClaims.Where(c => c.RoleId == role.Id).Select(c => c.ClaimValue).ToListAsync());
        foreach (var user in await check.Users.ToListAsync()) Assert.Equal(stamps[user.Id], user.SecurityStamp);
    }

    [SqlServerTheory]
    [InlineData("Cashier", false)]
    [InlineData("Cashier", true)]
    [InlineData("Admin", true)]
    [InlineData("SuperAdmin", true)]
    [InlineData("SuperAdmin,Admin", true)]
    public async Task Details_permissions_and_JWT_use_the_same_permission_policy(string roles, bool customized)
    {
        await using var database = await ModelIntegritySqlServerTests.TestDatabase.CreateAsync();
        var tenant = Guid.NewGuid();
        await using var f = await Fixture.CreateAsync(database.Context(tenant), tenant);
        await using var host = await IdentityHost.CreateAsync(f);
        var user = await host.CreateUserAsync("policy", roles.Split(','));
        var claims = new List<Claim> { new(Permissions.ClaimType, Permissions.Customers.View) };
        if (customized) claims.Add(new("Permissions.Customized", "true"));
        Assert.True((await host.Users.AddClaimsAsync(user, claims)).Succeeded);
        var details = await host.Service.GetUserByIdAsync(user.Id);
        var permissions = await host.Service.GetUserPermissionsAsync(user.Id);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["JwtSettings:Secret"] = "Identity-regression-tests-signing-key-only-123456789" }).Build();
        var auth = new AuthService(host.Users, host.Roles, config, f.Uow, f.Db, new ModelIntegrityTests.TestTenant(tenant));
        var login = await auth.LoginAsync(new() { UserName = user.UserName!, Password = Password });
        Assert.True(login.IsSuccess, login.ErrorMessage);
        Assert.Equal(details.Data!.EffectivePermissions, permissions.Data!.EffectivePermissions);
        Assert.Equal(details.Data.EffectivePermissions, login.Data!.Permissions);
        var jwtPermissions = new JwtSecurityTokenHandler().ReadJwtToken(login.Data.Token).Claims
            .Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).OrderBy(p => p).ToList();
        Assert.Equal(details.Data.EffectivePermissions, jwtPermissions);
        if (roles.Contains("SuperAdmin")) Assert.Contains(Permissions.Roles.Manage, jwtPermissions);
        else if (roles == "Admin") Assert.DoesNotContain(Permissions.Roles.Manage, jwtPermissions);
        else if (customized) Assert.Equal([Permissions.Customers.View], jwtPermissions);
        else Assert.Contains(Permissions.Products.View, jwtPermissions);
    }

    private const string Password = "Regression_123!";

    private sealed class FaultPlan
    {
        public int FailUserUpdateAt { get; set; }
        public int FailRoleUpdateAt { get; set; }
        public int UserUpdates { get; set; }
        public int RoleUpdates { get; set; }
        public static IdentityResult Failure => IdentityResult.Failed(new IdentityError { Code = "ConcurrencyFailure", Description = "Injected concurrent write" });
    }

    private sealed class FaultUserStore(AppDbContext db, FaultPlan fault) : UserStore<ApplicationUser, IdentityRole, AppDbContext>(db)
    {
        public override Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct = default) =>
            ++fault.UserUpdates == fault.FailUserUpdateAt ? Task.FromResult(FaultPlan.Failure) : base.UpdateAsync(user, ct);
    }

    private sealed class FaultRoleStore(AppDbContext db, FaultPlan fault) : RoleStore<IdentityRole, AppDbContext, string>(db)
    {
        public override Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken ct = default) =>
            ++fault.RoleUpdates == fault.FailRoleUpdateAt ? Task.FromResult(FaultPlan.Failure) : base.UpdateAsync(role, ct);
    }

    private sealed class IdentityHost : IAsyncDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly Fixture _fixture;
        public FaultPlan Fault { get; } = new();
        public UserManager<ApplicationUser> Users { get; }
        public RoleManager<IdentityRole> Roles { get; }
        public UserService Service { get; }

        private IdentityHost(Fixture fixture)
        {
            _fixture = fixture;
            var services = new ServiceCollection();
            services.AddLogging(); services.AddSingleton(fixture.Db);
            services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
            services.AddScoped<IUserStore<ApplicationUser>>(_ => new FaultUserStore(fixture.Db, Fault));
            services.AddScoped<IRoleStore<IdentityRole>>(_ => new FaultRoleStore(fixture.Db, Fault));
            _provider = services.BuildServiceProvider();
            Users = _provider.GetRequiredService<UserManager<ApplicationUser>>();
            Roles = _provider.GetRequiredService<RoleManager<IdentityRole>>();
            Service = new(Users, Roles, fixture.Db, new ModelIntegrityTests.TestTenant(fixture.TenantId),
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
        }

        public static async Task<IdentityHost> CreateAsync(Fixture fixture)
        {
            var host = new IdentityHost(fixture);
            foreach (var name in new[] { "Cashier", "WarehouseKeeper", "Admin", "SuperAdmin" })
                Assert.True((await host.Roles.CreateAsync(new(name))).Succeeded);
            var role = (await host.Roles.FindByNameAsync("Cashier"))!;
            Assert.True((await host.Roles.AddClaimAsync(role, new(Permissions.ClaimType, Permissions.Products.View))).Succeeded);
            host.ResetFaultCounters();
            return host;
        }

        public async Task<ApplicationUser> CreateUserAsync(string name, params string[] roles)
        {
            var user = new ApplicationUser
            {
                UserName = name, Email = name + "@example.test", TenantId = _fixture.TenantId,
                BranchId = _fixture.Branch.Id, LockoutEnabled = true
            };
            Assert.True((await Users.CreateAsync(user, Password)).Succeeded);
            Assert.True((await Users.AddToRolesAsync(user, roles.Length == 0 ? ["Cashier"] : roles)).Succeeded);
            ResetFaultCounters();
            return user;
        }

        public void ResetFaultCounters() { Fault.UserUpdates = 0; Fault.RoleUpdates = 0; }
        public ValueTask DisposeAsync() => _provider.DisposeAsync();
    }
}

internal sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RETAL_TEST_SQLSERVER")))
            Skip = "Set RETAL_TEST_SQLSERVER to an isolated SQL Server instance.";
    }
}
