using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Games.V1.ListGames;

public sealed class ListGamesHandler(IGameRepository games)
{
    public async Task<IReadOnlyList<GameCardResponse>> HandleAsync(ListGamesRequest request, CancellationToken ct)
    {
        var sort = request.Sort switch
        {
            "trending" => CatalogSort.Trending,
            "most-played" => CatalogSort.MostPlayed,
            "most-played-30d" => CatalogSort.MostPlayed30d,
            "top" => CatalogSort.Top,
            _ => CatalogSort.New,
        };
        var query = new CatalogQuery(sort, request.Tag, request.Q, Math.Clamp(request.Limit ?? 24, 1, 100));

        var cards = await games.ListCatalogAsync(query, ct);
        return cards.Select(GameCardResponse.From).ToList();
    }
}
