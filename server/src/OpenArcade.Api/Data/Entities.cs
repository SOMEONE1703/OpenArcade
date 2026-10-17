namespace OpenArcade.Api.Data;

public enum UserStatus { Active, Banned }
public enum RequestStatus { Pending, Approved, Rejected }
public enum PublisherStatus { Active, Suspended }
public enum GameStatus { Draft, InReview, Listed, Hidden, Delisted }
public enum MediaKind { Thumbnail, Screenshot, Video }
public enum CheckKind { Health, Change, Domain }
public enum VoteValue { Dislike = -1, Like = 1 }
public enum CommentStatus { Visible, Hidden, Deleted }
public enum ReportTarget { Game, Comment }
public enum ReportStatus { Open, Resolved, Dismissed }

public class User
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = "";
    public string Username { get; set; } = "";
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class PublisherRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string StudioName { get; set; } = "";
    public List<string> Links { get; set; } = [];
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public Guid? ReviewedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Publisher
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string StudioName { get; set; } = "";
    public PublisherStatus Status { get; set; } = PublisherStatus.Active;
}

public class Game
{
    public Guid Id { get; set; }
    public Guid PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string PlayUrl { get; set; } = "";
    public string Domain { get; set; } = "";
    /// <summary>Token the publisher puts in DNS TXT or /.well-known/openarcade.txt.</summary>
    public string VerificationToken { get; set; } = "";
    public DateTimeOffset? DomainVerifiedAt { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Draft;
    public bool MobileFriendly { get; set; }
    public int ConsecutiveHealthFailures { get; set; }

    // Cached counts, refreshed by StatsJobs.
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int PlayCount { get; set; }
    public int PlayCount30d { get; set; }
    public double TrendingScore { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<GameMedia> Media { get; set; } = [];
}

public class GameMedia
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public MediaKind Kind { get; set; }
    public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public class GameCheck
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public CheckKind Kind { get; set; }
    public bool Passed { get; set; }
    public string Result { get; set; } = "";
    public string? ScreenshotUrl { get; set; }
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>One row per game, visitor and day, so refreshes don't inflate counts.</summary>
public class Play
{
    public Guid GameId { get; set; }
    public string VisitorId { get; set; } = "";
    public Guid? UserId { get; set; }
    public DateOnly Day { get; set; }
}

public class Vote
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public VoteValue Value { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Favorite
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public Game Game { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class Comment
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public string Body { get; set; } = "";
    public int LikeCount { get; set; }
    public bool Pinned { get; set; }
    public CommentStatus Status { get; set; } = CommentStatus.Visible;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class CommentLike
{
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
}

public class Report
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public ReportTarget TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Reason { get; set; } = "";
    public ReportStatus Status { get; set; } = ReportStatus.Open;
    public Guid? HandledBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
