using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Services;

public sealed class LoginIdentifierValidator(AppDbContext db) : IUserValidator<ApplicationUser>
{
    public const string DuplicateCode = "DuplicateLoginIdentifier";
    public static IdentityError DuplicateError() => new()
    {
        Code = DuplicateCode,
        Description = "اسم المستخدم أو البريد الإلكتروني مستخدم كمعرف دخول لحساب آخر."
    };

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var identifiers = new[] { manager.NormalizeName(user.UserName), manager.NormalizeEmail(user.Email) }
            .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
        return await db.Users.AnyAsync(other => other.Id != user.Id &&
            (identifiers.Contains(other.NormalizedUserName) || identifiers.Contains(other.NormalizedEmail)))
            ? IdentityResult.Failed(DuplicateError()) : IdentityResult.Success;
    }

    public static bool IsDuplicate(IdentityResult result) => result.Errors.Any(e =>
        e.Code is DuplicateCode or "DuplicateUserName" or "DuplicateEmail");
}
