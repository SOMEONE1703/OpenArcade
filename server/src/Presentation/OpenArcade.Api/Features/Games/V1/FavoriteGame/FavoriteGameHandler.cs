using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Games.V1.FavoriteGame;

public sealed class FavoriteGameHandler(
    IGameRepository games, IFavoriteRepository favorites, IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result> AddAsync(string slug, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        if (!await favorites.ExistsAsync(user.Id, game.Id, ct))
        {
            favorites.Add(new Favorite(user.Id, game.Id, clock.UtcNow));
            await uow.SaveChangesAsync(ct);
        }
        return Result.Ok();
    }

    public async Task<Result> RemoveAsync(string slug, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var game = await games.GetBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        await favorites.RemoveAsync(user.Id, game.Id, ct);
        return Result.Ok();
    }
}
