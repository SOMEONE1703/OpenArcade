using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.SubmitGame;

/// <summary>Sends a verified, embeddable game to the admin review queue.</summary>
public sealed class SubmitGameHandler(
    IGameRepository games, IPublisherRepository publishers, IEmbedChecker checker, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result<PublisherGameResponse>> HandleAsync(Guid id, CancellationToken ct)
    {
        var owned = await PublisherAccess.OwnedGameAsync(id, games, publishers, current, ct);
        if (owned.Error is { } error) return error;
        var game = owned.Value!;

        var embed = await checker.CheckAsync(game.PlayUrl, ct);
        if (!embed.Allowed) return Error.Invalid(embed.Detail);

        game.SubmitForReview();
        games.Update(game);
        await uow.SaveChangesAsync(ct);
        return PublisherGameResponse.From(game);
    }
}
