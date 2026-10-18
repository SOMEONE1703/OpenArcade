using OpenArcade.Domain.Common;

namespace OpenArcade.Domain.Publishers;

public enum RequestStatus { Pending, Approved, Rejected }

/// <summary>A signed-in user's request for publisher access, reviewed once by an admin.</summary>
public sealed class PublisherRequest
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string StudioName { get; internal set; } = "";
    public IReadOnlyList<string> Links { get; internal set; } = [];
    public RequestStatus Status { get; internal set; }
    public Guid? ReviewedBy { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }

    internal PublisherRequest() { }

    public static PublisherRequest Submit(Guid userId, string studioName, IEnumerable<string>? links, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(studioName)) throw new DomainRuleException("Studio name is required.");
        return new PublisherRequest
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            StudioName = studioName.Trim(),
            Links = (links ?? []).Select(l => l.Trim()).Where(l => l.Length > 0).ToList(),
            Status = RequestStatus.Pending,
            CreatedAt = now,
        };
    }

    /// <summary>Approves the request and creates the publisher it grants.</summary>
    public Publisher Approve(Guid adminId)
    {
        EnsurePending();
        Status = RequestStatus.Approved;
        ReviewedBy = adminId;
        return Publisher.Create(UserId, StudioName);
    }

    public void Reject(Guid adminId)
    {
        EnsurePending();
        Status = RequestStatus.Rejected;
        ReviewedBy = adminId;
    }

    private void EnsurePending()
    {
        if (Status != RequestStatus.Pending) throw new DomainRuleException($"The request is already {Status}.");
    }
}
