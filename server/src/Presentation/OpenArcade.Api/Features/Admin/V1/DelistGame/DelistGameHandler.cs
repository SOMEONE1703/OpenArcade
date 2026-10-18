using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.DelistGame;

/// <summary>Kill switch: removes the game's play page immediately.</summary>
public sealed class DelistGameHandler(IGameRepository games, IUnitOfWork uow)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct)
    {
        var game = await games.GetAsync(id, ct);
        if (game is null) return Error.NotFound();

        game.Delist();
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
