using OpenArcade.Api.Data;

namespace OpenArcade.Api.Endpoints;

public record GameSummary(
    Guid Id, string Slug, string Title, string? ThumbnailUrl, List<string> Tags, string Studio,
    int LikeCount, int DislikeCount, int? LikePercent, int PlayCount, int PlayCount30d, bool MobileFriendly);

public record GameDetail(
    Guid Id, string Slug, string Title, string Description, List<string> Tags, string Studio,
    string PlayUrl, string Domain, bool MobileFriendly,
    int LikeCount, int DislikeCount, int? LikePercent, int PlayCount,
    List<MediaDto> Media, string? MyVote, bool IsFavorite, bool CanModerate);

public record MediaDto(string Kind, string Url);

public record CommentDto(
    Guid Id, Guid? ParentId, string Username, string? AvatarUrl, string Body, int LikeCount,
    bool Pinned, bool LikedByMe, bool Mine, int ReplyCount, DateTimeOffset CreatedAt);

public record CommentPage(List<CommentDto> Items, int HiddenCount);

public record MeDto(Guid Id, string Username, string? AvatarUrl, List<string> Roles, bool IsPublisher, string? PublisherRequestStatus);

public record ProfileDto(string Username, string? AvatarUrl, DateTimeOffset JoinedAt, List<GameSummary> Favorites);

public record PublisherGameDto(
    Guid Id, string Slug, string Title, string Description, List<string> Tags, string PlayUrl,
    string Domain, string Status, string VerificationToken, bool DomainVerified, bool MobileFriendly);

// Requests
public record VoteRequest(string Value);
public record ReportRequest(string Reason);
public record NewCommentRequest(string Body, Guid? ParentId);
public record PublisherApplication(string StudioName, List<string>? Links);
public record UpsertGameRequest(
    string Slug, string Title, string Description, List<string>? Tags, string PlayUrl,
    string? ThumbnailUrl, List<string>? Screenshots, bool MobileFriendly);

public static class Mapping
{
    public static int? LikePercent(int likes, int dislikes) =>
        likes + dislikes == 0 ? null : (int)Math.Round(100.0 * likes / (likes + dislikes));

    public static IQueryable<GameSummary> ToSummaries(this IQueryable<Game> q) => q.Select(g => new GameSummary(
        g.Id, g.Slug, g.Title,
        g.Media.Where(m => m.Kind == MediaKind.Thumbnail).Select(m => m.Url).FirstOrDefault(),
        g.Tags, g.Publisher.StudioName,
        g.LikeCount, g.DislikeCount,
        g.LikeCount + g.DislikeCount == 0 ? null : (int)Math.Round(100.0 * g.LikeCount / (g.LikeCount + g.DislikeCount)),
        g.PlayCount, g.PlayCount30d, g.MobileFriendly));

    public static PublisherGameDto ToPublisherDto(this Game g) => new(
        g.Id, g.Slug, g.Title, g.Description, g.Tags, g.PlayUrl, g.Domain, g.Status.ToString(),
        g.VerificationToken, g.DomainVerifiedAt is not null, g.MobileFriendly);
}
