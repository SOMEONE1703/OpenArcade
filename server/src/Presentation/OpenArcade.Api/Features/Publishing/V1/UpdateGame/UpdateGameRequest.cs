namespace OpenArcade.Api.Features.Publishing.V1.UpdateGame;

/// <summary>Replaces the listing. Changing the play URL or domain sends a listed game back to review.</summary>
public sealed record UpdateGameRequest(
    string Slug, string Title, string? Description, IReadOnlyList<string>? Tags, string PlayUrl,
    string? ThumbnailUrl, IReadOnlyList<string>? Screenshots, bool MobileFriendly) : IGameListingRequest;
