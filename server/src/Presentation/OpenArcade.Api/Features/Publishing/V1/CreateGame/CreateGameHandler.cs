using System.Security.Cryptography;
using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.CreateGame;

/// <summary>Registers a draft game. Next steps: verify the domain, test embedding, submit for review.</summary>
public sealed class CreateGameHandler(
    IGameRepository games, IPublisherRepository publishers, IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result<PublisherGameResponse>> HandleAsync(CreateGameRequest request, CancellationToken ct)
    {
        var publisher = await PublisherAccess.CurrentAsync(publishers, current, ct);
        if (publisher.Error is { } error) return error;
        if (await games.SlugExistsAsync(request.Slug, ct)) return Error.Conflict("That slug is taken.");

        var token = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
        var game = Game.Register(publisher.Value!.Id, request.ToListing(), token, clock.UtcNow);
        games.Add(game);
        await uow.SaveChangesAsync(ct);
        return PublisherGameResponse.From(game);
    }
}
