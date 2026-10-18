using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.LikeComment;

public sealed class LikeCommentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/comments/{id:guid}/like", async (Guid id, LikeCommentHandler handler, CancellationToken ct) =>
                (await handler.LikeAsync(id, ct)).ToHttp())
            .RequireAuthorization();

        app.MapDelete("/comments/{id:guid}/like", async (Guid id, LikeCommentHandler handler, CancellationToken ct) =>
                (await handler.UnlikeAsync(id, ct)).ToHttp())
            .RequireAuthorization();
    }
}
