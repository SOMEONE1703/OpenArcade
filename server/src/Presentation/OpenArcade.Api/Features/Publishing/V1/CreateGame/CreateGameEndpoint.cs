using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Publishing.V1.CreateGame;

public sealed class CreateGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/publisher/games", async (CreateGameRequest request, CreateGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(request, ct)).ToHttp(g => TypedResults.Created($"/api/v1/publisher/games/{g.Id}", g)))
            .RequireAuthorization();
}
