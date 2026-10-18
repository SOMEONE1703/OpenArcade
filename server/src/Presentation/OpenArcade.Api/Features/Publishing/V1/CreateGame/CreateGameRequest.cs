namespace OpenArcade.Api.Features.Publishing.V1.CreateGame;

public sealed record CreateGameRequest(
    string Slug, string Title, string? Description, IReadOnlyList<string>? Tags, string PlayUrl,
    string? ThumbnailUrl, IReadOnlyList<string>? Screenshots, bool MobileFriendly) : IGameListingRequest;
