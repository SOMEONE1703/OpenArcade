using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Publishing.V1;

/// <summary>The listing fields shared by the create and update requests.</summary>
public interface IGameListingRequest
{
    string Slug { get; }
    string Title { get; }
    string? Description { get; }
    IReadOnlyList<string>? Tags { get; }
    string PlayUrl { get; }
    string? ThumbnailUrl { get; }
    IReadOnlyList<string>? Screenshots { get; }
    bool MobileFriendly { get; }
}

internal static class GameListingRequestExtensions
{
    public static GameListing ToListing(this IGameListingRequest r) => new(
        r.Slug, r.Title, r.Description ?? "", r.Tags ?? [], r.PlayUrl, r.MobileFriendly, r.ThumbnailUrl, r.Screenshots ?? []);
}
