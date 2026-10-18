using OpenArcade.Domain.Games;

namespace OpenArcade.Infrastructure.Persistence.Records;

public class GameRecord
{
    public Guid Id { get; set; }
    public Guid PublisherId { get; set; }
    public PublisherRecord Publisher { get; set; } = null!;
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public string PlayUrl { get; set; } = "";
    public string Domain { get; set; } = "";
    public string VerificationToken { get; set; } = "";
    public DateTimeOffset? DomainVerifiedAt { get; set; }
    public GameStatus Status { get; set; }
    public bool MobileFriendly { get; set; }
    public int ConsecutiveHealthFailures { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int PlayCount { get; set; }
    public int PlayCount30d { get; set; }
    public double TrendingScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<GameMediaRecord> Media { get; set; } = [];
}

public class GameMediaRecord
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public MediaKind Kind { get; set; }
    public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public class GameCheckRecord
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public CheckKind Kind { get; set; }
    public bool Passed { get; set; }
    public string Result { get; set; } = "";
    public string? ScreenshotUrl { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}

public class PlayRecord
{
    public Guid GameId { get; set; }
    public string VisitorId { get; set; } = "";
    public Guid? UserId { get; set; }
    public DateOnly Day { get; set; }
}
