using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;

namespace OpenArcade.Infrastructure.Persistence.Records;

// Database models. Only EF Core and the mappers see these; the rest of the app works with Domain entities.

public class UserRecord
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PublisherRequestRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserRecord User { get; set; } = null!;
    public string StudioName { get; set; } = "";
    public List<string> Links { get; set; } = [];
    public RequestStatus Status { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class PublisherRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserRecord User { get; set; } = null!;
    public string StudioName { get; set; } = "";
    public PublisherStatus Status { get; set; }
}
