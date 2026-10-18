using OpenArcade.Domain.Community;

namespace OpenArcade.Infrastructure.Persistence.Records;

public class VoteRecord
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public VoteValue Value { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class FavoriteRecord
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public GameRecord Game { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}

public class CommentRecord
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public UserRecord User { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public string Body { get; set; } = "";
    public int LikeCount { get; set; }
    public bool Pinned { get; set; }
    public CommentStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CommentLikeRecord
{
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
}

public class ReportRecord
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public ReportTarget TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = "";
    public ReportStatus Status { get; set; }
    public Guid? HandledBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
