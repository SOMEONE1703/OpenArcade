using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.ListReviewQueue;

public sealed class ListReviewQueueHandler(IGameRepository games)
{
    /// <summary>Games waiting for an admin to play them and check content, desktop and mobile, and the listing.</summary>
    public async Task<IReadOnlyList<ListReviewQueueResponse>> HandleAsync(CancellationToken ct) =>
        (await games.ListReviewQueueAsync(ct))
            .Select(g => new ListReviewQueueResponse(g.Id, g.Slug, g.Title, g.PlayUrl, g.Domain, g.Studio, g.DomainVerifiedAt))
            .ToList();
}
