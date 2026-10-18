using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Account.V1.ApplyAsPublisher;

public sealed class ApplyAsPublisherEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/me/publisher-request", async (ApplyAsPublisherRequest request, ApplyAsPublisherHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(request, ct)).ToHttp(TypedResults.Accepted((string?)null)))
            .RequireAuthorization();
}
