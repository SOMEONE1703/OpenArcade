namespace OpenArcade.Domain.Publishers;

public interface IPublisherRepository
{
    Task<Publisher?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Publisher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    void Add(Publisher publisher);
}

/// <summary>A pending request with the applicant's username, for the admin queue.</summary>
public record PublisherRequestView(Guid Id, string Username, string StudioName, IReadOnlyList<string> Links, DateTimeOffset CreatedAt);

public interface IPublisherRequestRepository
{
    Task<PublisherRequest?> GetAsync(Guid id, CancellationToken ct = default);
    Task<PublisherRequest?> GetLatestForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<PublisherRequestView>> ListPendingAsync(CancellationToken ct = default);
    void Add(PublisherRequest request);
    void Update(PublisherRequest request);
}
