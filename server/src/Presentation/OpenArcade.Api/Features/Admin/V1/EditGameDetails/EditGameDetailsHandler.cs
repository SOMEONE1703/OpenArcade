using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Admin.V1.EditGameDetails;

/// <summary>Fixes a game's catalog details without sending it back to review.</summary>
public sealed class EditGameDetailsHandler(IGameRepository games, IUnitOfWork uow)
{
    public async Task<Result<AdminGameResponse>> HandleAsync(Guid id, EditGameDetailsRequest request, CancellationToken ct)
    {
        var game = await games.GetAsync(id, ct);
        if (game is null) return Error.NotFound();

        game.EditCatalogDetails(
            request.Title, request.Description ?? "", request.Tags ?? [], request.MobileFriendly, request.ThumbnailUrl,
            request.Screenshots ?? []);
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return AdminGameResponse.From(game);
    }
}
