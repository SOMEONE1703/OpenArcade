namespace OpenArcade.Domain.Abstractions;

/// <summary>Commits everything the repositories have added or changed in one transaction.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
