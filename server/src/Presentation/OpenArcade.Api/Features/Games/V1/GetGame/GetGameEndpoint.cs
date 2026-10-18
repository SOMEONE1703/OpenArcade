using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Games.V1.GetGame;

public sealed class GetGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/games/{slug}", async (string slug, GetGameHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(slug, ct)).ToHttp());
}
