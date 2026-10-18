using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Publishing.V1.ListMyGames;

public sealed class ListMyGamesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/publisher/games", async (ListMyGamesHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(ct)).ToHttp())
            .RequireAuthorization();
}
