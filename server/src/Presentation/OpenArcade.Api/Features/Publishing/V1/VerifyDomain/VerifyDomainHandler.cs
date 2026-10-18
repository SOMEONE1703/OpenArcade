using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Publishing.V1.VerifyDomain;

/// <summary>Checks for the token in DNS TXT or /.well-known/openarcade.txt, so nobody can submit someone else's game.</summary>
public sealed class VerifyDomainHandler(
    IGameRepository games, IPublisherRepository publishers, IGameCheckRepository checks, IDomainVerifier verifier,
    IUnitOfWork uow, CurrentUser current, IClock clock)
{
    public async Task<Result<VerifyDomainResponse>> HandleAsync(Guid id, CancellationToken ct)
    {
        var owned = await PublisherAccess.OwnedGameAsync(id, games, publishers, current, ct);
        if (owned.Error is { } error) return error;
        var game = owned.Value!;

        var ok = await verifier.VerifyAsync(game.PlayUrl, game.VerificationToken, ct);
        checks.Add(GameCheck.Record(game.Id, CheckKind.Domain, ok, ok ? "verified" : "token not found", clock.UtcNow));
        if (ok)
        {
            game.MarkDomainVerified(clock.UtcNow);
            games.Update(game);
        }
        await uow.SaveChangesAsync(ct);

        return new VerifyDomainResponse(ok,
            $"Add a DNS TXT record \"openarcade-verify={game.VerificationToken}\" on {game.Domain}, " +
            $"or serve {game.VerificationToken} at https://{game.Domain}/.well-known/openarcade.txt");
    }
}
