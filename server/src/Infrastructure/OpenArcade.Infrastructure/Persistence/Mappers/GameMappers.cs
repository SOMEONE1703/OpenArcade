using OpenArcade.Domain.Games;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Mappers;

internal static class GameMappers
{
    /// <summary>Requires <see cref="GameRecord.Media"/> to be loaded.</summary>
    public static Game ToDomain(this GameRecord r) => new()
    {
        Id = r.Id,
        PublisherId = r.PublisherId,
        Slug = r.Slug,
        Title = r.Title,
        Description = r.Description,
        Tags = [.. r.Tags],
        PlayUrl = r.PlayUrl,
        Domain = r.Domain,
        VerificationToken = r.VerificationToken,
        DomainVerifiedAt = r.DomainVerifiedAt,
        Status = r.Status,
        MobileFriendly = r.MobileFriendly,
        ConsecutiveHealthFailures = r.ConsecutiveHealthFailures,
        LikeCount = r.LikeCount,
        DislikeCount = r.DislikeCount,
        PlayCount = r.PlayCount,
        PlayCount30d = r.PlayCount30d,
        TrendingScore = r.TrendingScore,
        CreatedAt = r.CreatedAt,
        Media = r.Media.OrderBy(m => m.SortOrder).Select(m => new GameMedia(m.Kind, m.Url, m.SortOrder)).ToList(),
    };

    /// <summary>Copies scalar fields. Media are synced separately because they are child rows.</summary>
    public static void CopyTo(this Game g, GameRecord r)
    {
        r.Id = g.Id;
        r.PublisherId = g.PublisherId;
        r.Slug = g.Slug;
        r.Title = g.Title;
        r.Description = g.Description;
        r.Tags = [.. g.Tags];
        r.PlayUrl = g.PlayUrl;
        r.Domain = g.Domain;
        r.VerificationToken = g.VerificationToken;
        r.DomainVerifiedAt = g.DomainVerifiedAt;
        r.Status = g.Status;
        r.MobileFriendly = g.MobileFriendly;
        r.ConsecutiveHealthFailures = g.ConsecutiveHealthFailures;
        r.LikeCount = g.LikeCount;
        r.DislikeCount = g.DislikeCount;
        r.PlayCount = g.PlayCount;
        r.PlayCount30d = g.PlayCount30d;
        r.TrendingScore = g.TrendingScore;
        r.CreatedAt = g.CreatedAt;
    }

    public static GameMediaRecord ToRecord(this GameMedia m, Guid gameId) => new()
    {
        Id = Guid.CreateVersion7(), GameId = gameId, Kind = m.Kind, Url = m.Url, SortOrder = m.SortOrder,
    };

    public static GameCheckRecord ToRecord(this GameCheck c) => new()
    {
        Id = c.Id, GameId = c.GameId, Kind = c.Kind, Passed = c.Passed, Result = c.Result,
        ScreenshotUrl = c.ScreenshotUrl, CheckedAt = c.CheckedAt,
    };

    /// <summary>Projection used by catalog queries, so shelves load in one SQL query.</summary>
    public static IQueryable<GameCard> ToCards(this IQueryable<GameRecord> q) => q.Select(g => new GameCard(
        g.Id, g.Slug, g.Title,
        g.Media.Where(m => m.Kind == MediaKind.Thumbnail).Select(m => m.Url).FirstOrDefault(),
        g.Tags, g.Publisher.StudioName,
        g.LikeCount, g.DislikeCount, g.PlayCount, g.PlayCount30d, g.MobileFriendly));
}
