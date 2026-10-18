using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Games.V1.ListGames;

public sealed class ListGamesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/games", async ([AsParameters] ListGamesRequest request, ListGamesHandler handler, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleAsync(request, ct)));
}
