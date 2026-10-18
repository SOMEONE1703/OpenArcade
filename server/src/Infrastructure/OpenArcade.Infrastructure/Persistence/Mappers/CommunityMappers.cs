using OpenArcade.Domain.Community;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Mappers;

internal static class CommunityMappers
{
    public static Vote ToDomain(this VoteRecord r) => new()
    {
        UserId = r.UserId, GameId = r.GameId, Value = r.Value, UpdatedAt = r.UpdatedAt,
    };

    public static void CopyTo(this Vote v, VoteRecord r)
    {
        r.UserId = v.UserId;
        r.GameId = v.GameId;
        r.Value = v.Value;
        r.UpdatedAt = v.UpdatedAt;
    }

    public static Comment ToDomain(this CommentRecord r) => new()
    {
        Id = r.Id, GameId = r.GameId, UserId = r.UserId, ParentId = r.ParentId, Body = r.Body,
        LikeCount = r.LikeCount, Pinned = r.Pinned, Status = r.Status, CreatedAt = r.CreatedAt,
    };

    public static void CopyTo(this Comment c, CommentRecord r)
    {
        r.Id = c.Id;
        r.GameId = c.GameId;
        r.UserId = c.UserId;
        r.ParentId = c.ParentId;
        r.Body = c.Body;
        r.LikeCount = c.LikeCount;
        r.Pinned = c.Pinned;
        r.Status = c.Status;
        r.CreatedAt = c.CreatedAt;
    }

    public static Report ToDomain(this ReportRecord r) => new()
    {
        Id = r.Id, ReporterId = r.ReporterId, TargetType = r.TargetType, TargetId = r.TargetId,
        Reason = r.Reason, Status = r.Status, HandledBy = r.HandledBy, CreatedAt = r.CreatedAt,
    };

    public static void CopyTo(this Report rep, ReportRecord r)
    {
        r.Id = rep.Id;
        r.ReporterId = rep.ReporterId;
        r.TargetType = rep.TargetType;
        r.TargetId = rep.TargetId;
        r.Reason = rep.Reason;
        r.Status = rep.Status;
        r.HandledBy = rep.HandledBy;
        r.CreatedAt = rep.CreatedAt;
    }
}
