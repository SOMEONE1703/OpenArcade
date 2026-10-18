using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Games.V1.ListTags;

public sealed class ListTagsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/games/tags", async (ListTagsHandler handler, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleAsync(ct)));
}
