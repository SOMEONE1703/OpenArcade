using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Admin.V1.ReviewPublisherRequest;

public sealed class ReviewPublisherRequestEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/publisher-requests/{id:guid}/approve", async (Guid id, ReviewPublisherRequestHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, approve: true, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);

        app.MapPost("/admin/publisher-requests/{id:guid}/reject", async (Guid id, ReviewPublisherRequestHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, approve: false, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
    }
}
