using OpenArcade.Domain.Games;
using OpenArcade.Domain.Ranking;

namespace OpenArcade.Api.Features.Games.V1;

/// <summary>A game on a catalog shelf or a profile's favorites. Shared by the Games and Account features.</summary>
public sealed record GameCardResponse(
    Guid Id, string Slug, string Title, string? ThumbnailUrl, IReadOnlyList<string> Tags, string Studio,
    int LikeCount, int DislikeCount, int? LikePercent, int PlayCount, int PlayCount30d, bool MobileFriendly)
{
    public static GameCardResponse From(GameCard g) => new(
        g.Id, g.Slug, g.Title, g.ThumbnailUrl, g.Tags, g.Studio, g.LikeCount, g.DislikeCount,
        WilsonScore.LikePercent(g.LikeCount, g.DislikeCount), g.PlayCount, g.PlayCount30d, g.MobileFriendly);
}
