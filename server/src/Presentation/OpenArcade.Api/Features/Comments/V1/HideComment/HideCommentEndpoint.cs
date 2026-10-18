using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.HideComment;

public sealed class HideCommentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/comments/{id:guid}/hide", async (Guid id, HideCommentHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, hide: true, ct)).ToHttp())
            .RequireAuthorization();

        app.MapPost("/comments/{id:guid}/unhide", async (Guid id, HideCommentHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, hide: false, ct)).ToHttp())
            .RequireAuthorization();
    }
}
