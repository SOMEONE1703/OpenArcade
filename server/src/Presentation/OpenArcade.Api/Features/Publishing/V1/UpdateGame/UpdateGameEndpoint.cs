using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Publishing.V1.UpdateGame;

public sealed class UpdateGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/publisher/games/{id:guid}", async (Guid id, UpdateGameRequest request, UpdateGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, request, ct)).ToHttp())
            .RequireAuthorization();
}
