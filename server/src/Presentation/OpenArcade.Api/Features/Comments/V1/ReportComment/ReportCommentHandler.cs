using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Comments.V1.ReportComment;

public sealed class ReportCommentHandler(
    ICommentRepository comments, IReportRepository reports, IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result> HandleAsync(Guid id, ReportCommentRequest request, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (await comments.GetAsync(id, ct) is null) return Error.NotFound();

        reports.Add(Report.File(user.Id, ReportTarget.Comment, id, request.Reason, clock.UtcNow));
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
