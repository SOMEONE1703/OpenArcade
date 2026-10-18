using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.ReportComment;

public sealed class ReportCommentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/comments/{id:guid}/reports", async (Guid id, ReportCommentRequest request, ReportCommentHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, request, ct)).ToHttp(TypedResults.Accepted((string?)null)))
            .RequireAuthorization();
}
