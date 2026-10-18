using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;

namespace OpenArcade.Api.Features.Games.V1.ReportGame;

public sealed class ReportGameEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/games/{slug}/reports", async (string slug, ReportGameRequest request, ReportGameHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(slug, request, ct)).ToHttp(TypedResults.Accepted((string?)null)))
            .RequireAuthorization();
}
