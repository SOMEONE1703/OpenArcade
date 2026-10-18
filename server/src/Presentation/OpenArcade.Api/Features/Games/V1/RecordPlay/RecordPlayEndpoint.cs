using Microsoft.AspNetCore.Mvc;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Games.V1.RecordPlay;

public sealed class RecordPlayEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/games/{slug}/plays", async (
                string slug, [FromHeader(Name = "X-Visitor-Id")] string? visitorId, RecordPlayHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(slug, visitorId, ct)).ToHttp());
}
