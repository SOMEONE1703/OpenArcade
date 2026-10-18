using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1;

/// <summary>Resolves the signed-in user's publisher, and games that publisher owns.</summary>
internal static class PublisherAccess
{
    public static async Task<Result<Publisher>> CurrentAsync(IPublisherRepository publishers, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var publisher = await publishers.GetByUserIdAsync(user.Id, ct);
        return publisher is { IsActive: true } ? publisher : Error.Forbidden("You are not an approved publisher.");
    }

    public static async Task<Result<Game>> OwnedGameAsync(
        Guid gameId, IGameRepository games, IPublisherRepository publishers, CurrentUser current, CancellationToken ct)
    {
        var publisher = await CurrentAsync(publishers, current, ct);
        if (publisher.Error is { } error) return error;

        var game = await games.GetAsync(gameId, ct);
        // An archived game was removed by an admin, so it's gone for the publisher too.
        return game is not null && game.PublisherId == publisher.Value!.Id && game.Status != GameStatus.Archived
            ? game
            : Error.NotFound();
    }
}
