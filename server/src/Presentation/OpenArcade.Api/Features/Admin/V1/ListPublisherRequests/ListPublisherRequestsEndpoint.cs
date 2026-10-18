using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Admin.V1.ListPublisherRequests;

public sealed class ListPublisherRequestsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/admin/publisher-requests", async (ListPublisherRequestsHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .RequireAuthorization(Roles.Admin);
}
