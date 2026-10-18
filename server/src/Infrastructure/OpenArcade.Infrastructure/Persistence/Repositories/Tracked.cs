using Microsoft.EntityFrameworkCore;

namespace OpenArcade.Infrastructure.Persistence.Repositories;

internal static class Tracked
{
    /// <summary>
    /// Finds the record a repository loaded earlier in this request, so Update can copy the entity's changes onto it.
    /// Entities must be loaded through a repository before they are updated.
    /// </summary>
    public static TRecord Find<TRecord>(DbSet<TRecord> set, Func<TRecord, bool> match) where TRecord : class =>
        set.Local.FirstOrDefault(match)
        ?? throw new InvalidOperationException($"Load the {typeof(TRecord).Name} through its repository before updating it.");
}
