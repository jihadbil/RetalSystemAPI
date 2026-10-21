using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Constants;

namespace RetalSystemAPI.Services.Auth.Implementations;

internal sealed record UserPermissionSnapshot(List<string> Roles, bool IsCustomized,
    List<string> DirectPermissions, List<string> RolePermissions, List<string> EffectivePermissions);

internal static class UserPermissionResolver
{
    internal static async Task<UserPermissionSnapshot> ResolveAsync(ApplicationUser user,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var claims = await userManager.GetClaimsAsync(user);
        var customized = claims.Any(c => c.Type == "Permissions.Customized" && c.Value == "true");
        var direct = PermissionValues(claims);
        var inherited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in roles)
        {
            var role = await roleManager.FindByNameAsync(name);
            if (role is not null) inherited.UnionWith(PermissionValues(await roleManager.GetClaimsAsync(role)));
        }

        IEnumerable<string> effective;
        if (roles.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase))
            effective = AppPermissions.GetAllPermissionCodes();
        else if (roles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            effective = AppPermissions.GetAllPermissionCodes().Where(p => p != Permissions.Roles.Manage);
        else
            effective = customized ? direct : inherited.Concat(direct);

        return new(roles, customized, direct, inherited.OrderBy(p => p).ToList(),
            effective.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList());
    }

    private static List<string> PermissionValues(IEnumerable<Claim> claims) => claims
        .Where(c => c.Type == Permissions.ClaimType && !string.IsNullOrWhiteSpace(c.Value))
        .Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
}
