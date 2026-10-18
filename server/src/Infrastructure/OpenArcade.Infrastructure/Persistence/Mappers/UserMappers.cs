using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Mappers;

internal static class UserMappers
{
    public static User ToDomain(this UserRecord r) => new()
    {
        Id = r.Id, KeycloakId = r.KeycloakId, Username = r.Username, Email = r.Email,
        AvatarUrl = r.AvatarUrl, Status = r.Status, CreatedAt = r.CreatedAt,
    };

    public static void CopyTo(this User u, UserRecord r)
    {
        r.Id = u.Id;
        r.KeycloakId = u.KeycloakId;
        r.Username = u.Username;
        r.Email = u.Email;
        r.AvatarUrl = u.AvatarUrl;
        r.Status = u.Status;
        r.CreatedAt = u.CreatedAt;
    }

    public static Publisher ToDomain(this PublisherRecord r) => new()
    {
        Id = r.Id, UserId = r.UserId, StudioName = r.StudioName, Status = r.Status,
    };

    public static void CopyTo(this Publisher p, PublisherRecord r)
    {
        r.Id = p.Id;
        r.UserId = p.UserId;
        r.StudioName = p.StudioName;
        r.Status = p.Status;
    }

    public static PublisherRequest ToDomain(this PublisherRequestRecord r) => new()
    {
        Id = r.Id, UserId = r.UserId, StudioName = r.StudioName, Links = [.. r.Links],
        Status = r.Status, ReviewedBy = r.ReviewedBy, CreatedAt = r.CreatedAt,
    };

    public static void CopyTo(this PublisherRequest p, PublisherRequestRecord r)
    {
        r.Id = p.Id;
        r.UserId = p.UserId;
        r.StudioName = p.StudioName;
        r.Links = [.. p.Links];
        r.Status = p.Status;
        r.ReviewedBy = p.ReviewedBy;
        r.CreatedAt = p.CreatedAt;
    }
}
