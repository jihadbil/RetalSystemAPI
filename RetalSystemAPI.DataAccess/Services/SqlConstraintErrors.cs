using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RetalSystemAPI.DataAccess.Services;

public static class SqlConstraintErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception, params string[] constraints) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 } sql &&
        constraints.Any(name => sql.Message.Contains(name, StringComparison.OrdinalIgnoreCase));

    public static bool IsBranchInUse(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 51001 };
}
