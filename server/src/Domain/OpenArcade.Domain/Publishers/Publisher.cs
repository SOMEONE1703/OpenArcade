namespace OpenArcade.Domain.Publishers;

public enum PublisherStatus { Active, Suspended }

/// <summary>A user approved to list games on OpenArcade.</summary>
public sealed class Publisher
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }
    public string StudioName { get; internal set; } = "";
    public PublisherStatus Status { get; internal set; }

    internal Publisher() { }

    internal static Publisher Create(Guid userId, string studioName) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        StudioName = studioName,
        Status = PublisherStatus.Active,
    };

    public bool IsActive => Status == PublisherStatus.Active;
}
