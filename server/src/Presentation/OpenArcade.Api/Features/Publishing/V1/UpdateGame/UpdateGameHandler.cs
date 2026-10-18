using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.UpdateGame;

public sealed class UpdateGameHandler(IGameRepository games, IPublisherRepository publishers, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result<PublisherGameResponse>> HandleAsync(Guid id, UpdateGameRequest request, CancellationToken ct)
    {
        var owned = await PublisherAccess.OwnedGameAsync(id, games, publishers, current, ct);
        if (owned.Error is { } error) return error;
        var game = owned.Value!;

        if (game.Slug != request.Slug && await games.SlugExistsAsync(request.Slug, ct)) return Error.Conflict("That slug is taken.");

        game.UpdateListing(request.ToListing());
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return PublisherGameResponse.From(game);
    }
}
