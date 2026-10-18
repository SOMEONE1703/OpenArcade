using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.CheckEmbed;

/// <summary>Tests that the play URL lets OpenArcade frame it (X-Frame-Options and CSP frame-ancestors).</summary>
public sealed class CheckEmbedHandler(IGameRepository games, IPublisherRepository publishers, IEmbedChecker checker, CurrentUser current)
{
    public async Task<Result<CheckEmbedResponse>> HandleAsync(Guid id, CancellationToken ct)
    {
        var owned = await PublisherAccess.OwnedGameAsync(id, games, publishers, current, ct);
        if (owned.Error is { } error) return error;

        var result = await checker.CheckAsync(owned.Value!.PlayUrl, ct);
        return new CheckEmbedResponse(result.Allowed, result.Detail);
    }
}
