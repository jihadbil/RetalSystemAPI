using Microsoft.EntityFrameworkCore;
using System.Data;
using RetalSystemAPI.DataAccess.Context;

namespace RetalSystemAPI.DataAccess.Transactions;

/// <summary>Commits multi-save operations only when their complete logical result succeeds.</summary>
public static class AtomicWrite
{
    public static async Task<T> ExecuteAsync<T>(AppDbContext context, Func<Task<T>> operation,
        Func<T, bool> succeeded, CancellationToken ct = default)
    {
        context.EnsureTransactionUsable();
        await using var transaction = context.Database.IsRelational()
            ? await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct)
            : null;
        var completed = false;
        Exception? operationError = null;
        try
        {
            var result = await operation();
            if (succeeded(result))
            {
                if (transaction is not null) await transaction.CommitAsync(ct);
                completed = true;
            }
            return result;
        }
        catch (Exception ex)
        {
            operationError = ex;
            throw;
        }
        finally
        {
            if (!completed)
            {
                try
                {
                    if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
                }
                catch when (operationError is not null)
                {
                    // Preserve the operation error if the database already aborted the transaction.
                }
                finally
                {
                    context.InvalidateAfterRollback();
                }
            }
        }
    }
}
