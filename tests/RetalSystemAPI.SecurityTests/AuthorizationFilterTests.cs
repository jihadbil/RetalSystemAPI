using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Controllers.Auth;
using RetalSystemAPI.Controllers.Tenants;
using RetalSystemAPI.Models.Constants;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public class AuthorizationFilterTests
{
    [Fact]
    public async Task Anonymous_request_is_rejected_with_401()
    {
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity()));

        await new HasPermissionAttribute(Permissions.Products.View).OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_does_not_bypass_missing_permission()
    {
        var context = CreateContext(CreateUser(role: "Admin"));

        await new HasPermissionAttribute(Permissions.Roles.Manage).OnAuthorizationAsync(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task Super_admin_can_bypass_permission_check()
    {
        var context = CreateContext(CreateUser(role: "SuperAdmin"));

        await new HasPermissionAttribute(Permissions.Roles.Manage).OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Explicit_permission_allows_tenant_user()
    {
        var context = CreateContext(CreateUser(permission: Permissions.Products.View));

        await new HasPermissionAttribute(Permissions.Products.View).OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void Registration_endpoint_is_not_anonymous()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Register))!;

        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
    }

    [Fact]
    public void Cross_tenant_management_requires_super_admin()
    {
        var methodNames = new[]
        {
            nameof(TenantsController.GetAll),
            nameof(TenantsController.GetPaged),
            nameof(TenantsController.GetById),
            nameof(TenantsController.Create),
            nameof(TenantsController.Update),
            nameof(TenantsController.Delete),
            nameof(TenantsController.ToggleActiveStatus)
        };

        foreach (var methodName in methodNames)
        {
            var method = typeof(TenantsController).GetMethod(methodName)!;
            var authorization = method
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>();
            Assert.Contains(authorization, attribute => attribute.Roles == "SuperAdmin");
        }
    }

    private static AuthorizationFilterContext CreateContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext { User = user };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static ClaimsPrincipal CreateUser(string? role = null, string? permission = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "test-user") };
        if (role is not null) claims.Add(new Claim(ClaimTypes.Role, role));
        if (permission is not null) claims.Add(new Claim(Permissions.ClaimType, permission));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}
