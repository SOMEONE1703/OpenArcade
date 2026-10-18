using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Games.V1.ReportGame;

public sealed class ReportGameHandler(
    IGameRepository games, IReportRepository reports, IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result> HandleAsync(string slug, ReportGameRequest request, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        // Any status: players may report a game whose play page just stopped working.
        var game = await games.GetBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        reports.Add(Report.File(user.Id, ReportTarget.Game, game.Id, request.Reason, clock.UtcNow));
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
