using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.ListMyGames;

public sealed class ListMyGamesHandler(IGameRepository games, IPublisherRepository publishers, CurrentUser current)
{
    public async Task<Result<IReadOnlyList<PublisherGameResponse>>> HandleAsync(CancellationToken ct)
    {
        var publisher = await PublisherAccess.CurrentAsync(publishers, current, ct);
        if (publisher.Error is { } error) return error;

        var mine = await games.ListByPublisherAsync(publisher.Value!.Id, ct);
        return mine.Select(PublisherGameResponse.From).ToList();
    }
}
