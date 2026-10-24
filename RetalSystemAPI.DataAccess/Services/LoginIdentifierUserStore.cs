using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Services;

/// <summary>Convert races against the database login reservation into Identity validation failures.</summary>
public sealed class LoginIdentifierUserStore(AppDbContext db, IdentityErrorDescriber errors)
    : UserStore<ApplicationUser, IdentityRole, AppDbContext>(db, errors)
{
    public override Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct = default) =>
        SaveAsync(user, () => base.CreateAsync(user, ct));

    public override Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct = default) =>
        SaveAsync(user, () => base.UpdateAsync(user, ct));

    private async Task<IdentityResult> SaveAsync(ApplicationUser user, Func<Task<IdentityResult>> save)
    {
        try { return await save(); }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsUniqueViolation(ex,
            "PK_UserLoginIdentifiers", "UserNameIndex"))
        {
            Context.Entry(user).State = EntityState.Detached;
            return IdentityResult.Failed(LoginIdentifierValidator.DuplicateError());
        }
    }
}
