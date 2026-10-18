namespace OpenArcade.Api.Features.Admin.V1.ListAllGames;

public sealed record ListAllGamesResponse(
    Guid Id, string Slug, string Title, string? ThumbnailUrl, string Studio, string Status, string PlayUrl, string Domain,
    bool DomainVerified, int PlayCount, int LikeCount, int DislikeCount, int ConsecutiveHealthFailures,
    DateTimeOffset CreatedAt);
