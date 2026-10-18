using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.ReviewGame;

public sealed class ReviewGameHandler(IGameRepository games, IUnitOfWork uow)
{
    /// <summary>Lists the game, or sends it back to the publisher as a draft.</summary>
    public async Task<Result> HandleAsync(Guid id, bool approve, CancellationToken ct)
    {
        var game = await games.GetAsync(id, ct);
        if (game is null) return Error.NotFound();

        if (approve) game.Approve();
        else game.SendBackToDraft();
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
