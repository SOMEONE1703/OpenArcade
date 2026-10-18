using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Admin.V1.ListReports;

public sealed class ListReportsHandler(IReportRepository reports)
{
    /// <summary>The oldest 100 open reports.</summary>
    public async Task<IReadOnlyList<ListReportsResponse>> HandleAsync(CancellationToken ct) =>
        (await reports.ListOpenAsync(100, ct))
            .Select(r => new ListReportsResponse(r.Id, r.ReporterId, r.TargetType.ToString(), r.TargetId, r.Reason, r.CreatedAt))
            .ToList();
}
