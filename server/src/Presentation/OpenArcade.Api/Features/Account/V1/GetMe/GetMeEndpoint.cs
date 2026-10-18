using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Account.V1.GetMe;

public sealed class GetMeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/me", async (GetMeHandler handler, CancellationToken ct) => TypedResults.Ok(await handler.HandleAsync(ct)))
            .RequireAuthorization();
}
