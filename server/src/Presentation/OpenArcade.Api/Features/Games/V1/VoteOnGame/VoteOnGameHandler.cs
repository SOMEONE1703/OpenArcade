using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Ranking;

namespace OpenArcade.Api.Features.Games.V1.VoteOnGame;

public sealed class VoteOnGameHandler(
    IGameRepository games, IPublisherRepository publishers, IVoteRepository votes, IUnitOfWork uow,
    CurrentUser current, IClock clock)
{
    public async Task<Result<VoteOnGameResponse>> SetAsync(string slug, VoteOnGameRequest request, CancellationToken ct)
    {
        VoteValue value;
        switch (request.Value)
        {
            case "like": value = VoteValue.Like; break;
            case "dislike": value = VoteValue.Dislike; break;
            default: return Error.Invalid("Value must be like or dislike.");
        }

        var user = await current.RequireAsync(ct);
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        var existing = await votes.GetAsync(user.Id, game.Id, ct);
        if (existing is null)
        {
            var publisher = await publishers.GetAsync(game.PublisherId, ct) ?? throw new InvalidOperationException("Game has no publisher.");
            votes.Add(Vote.Cast(user, game, publisher, value, clock.UtcNow));
        }
        else
        {
            user.EnsureCanParticipate();
            existing.Change(value, clock.UtcNow);
            votes.Update(existing);
        }
        return await SaveAndRefreshCountsAsync(game, ct);
    }

    public async Task<Result<VoteOnGameResponse>> ClearAsync(string slug, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();

        var existing = await votes.GetAsync(user.Id, game.Id, ct);
        if (existing is not null) votes.Remove(existing);
        return await SaveAndRefreshCountsAsync(game, ct);
    }

    private async Task<VoteOnGameResponse> SaveAndRefreshCountsAsync(Game game, CancellationToken ct)
    {
        await uow.SaveChangesAsync(ct);

        var counts = await votes.CountAsync(game.Id, ct);
        game.SetVoteCounts(counts.Likes, counts.Dislikes);
        games.Update(game);
        await uow.SaveChangesAsync(ct);

        return new VoteOnGameResponse(counts.Likes, counts.Dislikes, WilsonScore.LikePercent(counts.Likes, counts.Dislikes));
    }
}
