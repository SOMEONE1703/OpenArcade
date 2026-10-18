using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.PinComment;

public sealed class PinCommentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/comments/{id:guid}/pin", async (Guid id, PinCommentHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ct)).ToHttp())
            .RequireAuthorization();
}
