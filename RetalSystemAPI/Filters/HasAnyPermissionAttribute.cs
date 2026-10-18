using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RetalSystemAPI.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class HasAnyPermissionAttribute(params string[] permissions) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (permissions.Length == 0) { context.Result = new ForbidResult(); return; }
        foreach (var permission in permissions)
        {
            context.Result = null;
            await new HasPermissionAttribute(permission).OnAuthorizationAsync(context);
            if (context.Result is null or ObjectResult { StatusCode: 401 }) return;
        }
    }
}
