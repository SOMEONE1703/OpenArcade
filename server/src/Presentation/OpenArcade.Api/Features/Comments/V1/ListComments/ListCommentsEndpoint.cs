using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Comments.V1.ListComments;

public sealed class ListCommentsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/games/{slug}/comments", async (
                string slug, [AsParameters] ListCommentsRequest request, ListCommentsHandler handler, CancellationToken ct) =>
            (await handler.HandleAsync(slug, request, ct)).ToHttp());
}
