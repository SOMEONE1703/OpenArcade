using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Publishing.V1.CheckEmbed;

public sealed class CheckEmbedEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/publisher/games/{id:guid}/check-embed", async (Guid id, CheckEmbedHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ct)).ToHttp())
            .RequireAuthorization();
}
