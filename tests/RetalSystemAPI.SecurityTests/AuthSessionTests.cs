using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Auth;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Responses;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

public sealed class AuthSessionTests
{
    [Fact]
    public void Session_requires_authentication_but_no_screen_permission()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Session))!;
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Empty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.Empty(method.GetCustomAttributes(typeof(HasPermissionAttribute), true));
        var cache = Assert.Single(method.GetCustomAttributes(typeof(ResponseCacheAttribute), true).Cast<ResponseCacheAttribute>());
        Assert.True(cache.NoStore);
    }

    [Fact]
    public void Session_returns_only_the_authenticated_identity_and_granted_claims()
    {
        var tenantId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var controller = Controller(
            new(ClaimTypes.NameIdentifier, "tenant-admin"), new(ClaimTypes.Name, "masoud"),
            new("TenantId", tenantId.ToString()), new("BranchId", branchId.ToString()),
            new(ClaimTypes.Role, "Admin"), new(Permissions.ClaimType, Permissions.Products.View),
            new("permission", Permissions.Products.View));

        var result = Assert.IsType<OkObjectResult>(controller.Session());
        var session = Assert.IsType<ApiResponse<SessionUserDto>>(result.Value).Data!;
        Assert.Equal("tenant-admin", session.Id);
        Assert.Equal("masoud", session.UserName);
        Assert.Equal(tenantId, session.TenantId);
        Assert.Equal(branchId, session.BranchId);
        Assert.Equal(["Admin"], session.Roles);
        Assert.Equal([Permissions.Products.View], session.Permissions);
        Assert.DoesNotContain(Permissions.Roles.Manage, session.Permissions);
    }

    [Fact]
    public void User_without_permissions_can_still_restore_the_session()
    {
        var result = Assert.IsType<OkObjectResult>(Controller(
            new(ClaimTypes.NameIdentifier, "limited"), new("TenantId", Guid.NewGuid().ToString()),
            new("BranchId", Guid.NewGuid().ToString())).Session());
        Assert.Empty(Assert.IsType<ApiResponse<SessionUserDto>>(result.Value).Data!.Permissions);
    }

    [Fact]
    public void Missing_identity_requires_signing_in_again()
    {
        Assert.IsType<UnauthorizedObjectResult>(Controller().Session());
        Assert.IsType<UnauthorizedObjectResult>(Controller(new Claim(ClaimTypes.NameIdentifier, "incomplete")).Session());
    }

    private static AuthController Controller(params Claim[] claims) => new(null!)
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) }
        }
    };
}
