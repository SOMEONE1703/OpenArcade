namespace OpenArcade.Api.Features.Games.V1.GetGame;

public sealed record GetGameResponse(
    Guid Id, string Slug, string Title, string Description, IReadOnlyList<string> Tags, string Studio,
    string PlayUrl, string Domain, bool MobileFriendly,
    int LikeCount, int DislikeCount, int? LikePercent, int PlayCount,
    IReadOnlyList<GetGameResponse.MediaItem> Media,
    string? MyVote, bool IsFavorite,
    /* True for the game's publisher and admins: they can pin and hide comments. */ bool CanModerate)
{
    public sealed record MediaItem(string Kind, string Url);
}
