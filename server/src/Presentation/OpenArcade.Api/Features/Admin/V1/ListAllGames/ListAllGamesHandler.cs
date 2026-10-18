using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.ListAllGames;

public sealed class ListAllGamesHandler(IGameRepository games)
{
    /// <summary>Every game in the catalog, newest first, whatever its status.</summary>
    public async Task<Result<IReadOnlyList<ListAllGamesResponse>>> HandleAsync(ListAllGamesRequest request, CancellationToken ct)
    {
        GameStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<GameStatus>(request.Status, ignoreCase: true, out var parsed)) return Error.Invalid("Unknown status.");
            status = parsed;
        }

        var query = new AdminGameQuery(status, request.Q?.Trim(), Math.Clamp(request.Limit ?? 100, 1, 500));
        return (await games.ListForAdminAsync(query, ct))
            .Select(g => new ListAllGamesResponse(
                g.Id, g.Slug, g.Title, g.ThumbnailUrl, g.Studio, g.Status.ToString(), g.PlayUrl, g.Domain, g.DomainVerified,
                g.PlayCount, g.LikeCount, g.DislikeCount, g.ConsecutiveHealthFailures, g.CreatedAt))
            .ToList();
    }
}
