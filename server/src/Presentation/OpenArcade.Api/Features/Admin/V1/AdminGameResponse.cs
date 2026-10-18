using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1;

/// <summary>A game of any status with everything an admin can see or edit.</summary>
public sealed record AdminGameResponse(
    Guid Id, string Slug, string Title, string Description, IReadOnlyList<string> Tags, string PlayUrl, string Domain,
    string Status, bool DomainVerified, bool MobileFriendly, string? ThumbnailUrl, IReadOnlyList<string> Screenshots,
    int PlayCount, int LikeCount, int DislikeCount, int ConsecutiveHealthFailures, DateTimeOffset CreatedAt)
{
    public static AdminGameResponse From(Game g) => new(
        g.Id, g.Slug, g.Title, g.Description, g.Tags, g.PlayUrl, g.Domain, g.Status.ToString(), g.IsDomainVerified,
        g.MobileFriendly, g.ThumbnailUrl,
        g.Media.Where(m => m.Kind == MediaKind.Screenshot).OrderBy(m => m.SortOrder).Select(m => m.Url).ToList(),
        g.PlayCount, g.LikeCount, g.DislikeCount, g.ConsecutiveHealthFailures, g.CreatedAt);
}
