using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Publishing.V1.SubmitGame;

public sealed class SubmitGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/publisher/games/{id:guid}/submit", async (Guid id, SubmitGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ct)).ToHttp())
            .RequireAuthorization();
}
