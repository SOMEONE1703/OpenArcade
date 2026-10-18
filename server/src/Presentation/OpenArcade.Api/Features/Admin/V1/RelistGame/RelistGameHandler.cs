using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.RelistGame;

/// <summary>Undoes a delist: the game's play page comes back.</summary>
public sealed class RelistGameHandler(IGameRepository games, IUnitOfWork uow)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct)
    {
        var game = await games.GetAsync(id, ct);
        if (game is null) return Error.NotFound();

        game.Relist();
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
