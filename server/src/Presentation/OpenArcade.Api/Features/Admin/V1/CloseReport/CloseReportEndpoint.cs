using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Endpoints;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Admin.V1.CloseReport;

public sealed class CloseReportEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/admin/reports/{id:guid}/resolve", async (Guid id, CloseReportHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ReportStatus.Resolved, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);

        app.MapPost("/admin/reports/{id:guid}/dismiss", async (Guid id, CloseReportHandler handler, CancellationToken ct) =>
                (await handler.HandleAsync(id, ReportStatus.Dismissed, ct)).ToHttp())
            .RequireAuthorization(Roles.Admin);
    }
}
