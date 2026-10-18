namespace OpenArcade.Api.Features.Admin.V1.EditGameDetails;

/// <summary>The catalog details an admin can change. The slug and play URL stay with the publisher.</summary>
public sealed record EditGameDetailsRequest(
    string Title, string? Description, IReadOnlyList<string>? Tags, string? ThumbnailUrl, IReadOnlyList<string>? Screenshots,
    bool MobileFriendly);
