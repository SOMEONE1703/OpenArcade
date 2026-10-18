using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Admin.V1.CloseReport;

public sealed class CloseReportHandler(IReportRepository reports, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result> HandleAsync(Guid id, ReportStatus outcome, CancellationToken ct)
    {
        var admin = await current.RequireAsync(ct);
        var report = await reports.GetAsync(id, ct);
        if (report is null) return Error.NotFound();

        report.Close(outcome, admin.Id);
        reports.Update(report);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
