using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Comments.V1.ListReplies;

public sealed class ListRepliesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/comments/{id:guid}/replies", async (Guid id, ListRepliesHandler handler, CancellationToken ct) =>
            TypedResults.Ok(await handler.HandleAsync(id, ct)));
}
