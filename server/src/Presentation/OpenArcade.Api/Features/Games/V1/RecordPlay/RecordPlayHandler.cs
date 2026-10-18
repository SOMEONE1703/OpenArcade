using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Games.V1.RecordPlay;

/// <summary>
/// Called by the play page when the game's iframe finishes loading. Counted at most once per visitor
/// (or signed-in user) per game per day.
/// </summary>
public sealed class RecordPlayHandler(IGameRepository games, IPlayRepository plays, CurrentUser current, IClock clock)
{
    public async Task<Result<RecordPlayResponse>> HandleAsync(string slug, string? visitorHeader, CancellationToken ct)
    {
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        var user = await current.GetAsync(ct);
        var visitorId = user is not null ? $"u:{user.Id}" : visitorHeader;
        if (string.IsNullOrWhiteSpace(visitorId) || visitorId.Length > 64) return Error.Invalid("Missing X-Visitor-Id.");

        var counted = await plays.TryRecordAsync(new Play(game.Id, visitorId, user?.Id, clock.Today), ct);
        return new RecordPlayResponse(counted);
    }
}
