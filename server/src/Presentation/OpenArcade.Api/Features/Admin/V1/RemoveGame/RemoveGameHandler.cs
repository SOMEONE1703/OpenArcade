using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.RemoveGame;

/// <summary>
/// Soft delete: archives a delisted game so it's gone everywhere, including the publisher's list, or restores an
/// archived one as delisted. Plays, votes and comments are kept.
/// </summary>
public sealed class RemoveGameHandler(IGameRepository games, IUnitOfWork uow)
{
    public async Task<Result> HandleAsync(Guid id, bool restore, CancellationToken ct)
    {
        var game = await games.GetAsync(id, ct);
        if (game is null) return Error.NotFound();

        if (restore) game.Restore();
        else game.Archive();
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
