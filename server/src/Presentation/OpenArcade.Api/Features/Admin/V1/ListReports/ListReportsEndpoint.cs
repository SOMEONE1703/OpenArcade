using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;

namespace OpenArcade.Api.Features.Admin.V1.ListReports;

public sealed class ListReportsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/admin/reports", async (ListReportsHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .RequireAuthorization(Roles.Admin);
}
