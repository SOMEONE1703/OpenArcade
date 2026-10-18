using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Publishing.V1;

/// <summary>A game as its publisher sees it, including review status and the domain verification token.</summary>
public sealed record PublisherGameResponse(
    Guid Id, string Slug, string Title, string Description, IReadOnlyList<string> Tags, string PlayUrl,
    string Domain, string Status, string VerificationToken, bool DomainVerified, bool MobileFriendly)
{
    public static PublisherGameResponse From(Game g) => new(
        g.Id, g.Slug, g.Title, g.Description, g.Tags, g.PlayUrl, g.Domain, g.Status.ToString(),
        g.VerificationToken, g.IsDomainVerified, g.MobileFriendly);
}
