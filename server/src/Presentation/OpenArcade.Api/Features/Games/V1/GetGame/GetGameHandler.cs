using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Ranking;

namespace OpenArcade.Api.Features.Games.V1.GetGame;

public sealed class GetGameHandler(
    IGameRepository games, IPublisherRepository publishers, IVoteRepository votes, IFavoriteRepository favorites,
    CurrentUser current)
{
    public async Task<Result<GetGameResponse>> HandleAsync(string slug, CancellationToken ct)
    {
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();
        var publisher = await publishers.GetAsync(game.PublisherId, ct) ?? throw new InvalidOperationException("Game has no publisher.");

        var user = await current.GetAsync(ct);
        string? myVote = null;
        var isFavorite = false;
        if (user is not null)
        {
            var vote = await votes.GetAsync(user.Id, game.Id, ct);
            myVote = vote?.Value switch { VoteValue.Like => "like", VoteValue.Dislike => "dislike", _ => null };
            isFavorite = await favorites.ExistsAsync(user.Id, game.Id, ct);
        }

        return new GetGameResponse(
            game.Id, game.Slug, game.Title, game.Description, game.Tags, publisher.StudioName,
            game.PlayUrl, game.Domain, game.MobileFriendly,
            game.LikeCount, game.DislikeCount, WilsonScore.LikePercent(game.LikeCount, game.DislikeCount), game.PlayCount,
            game.Media.Select(m => new GetGameResponse.MediaItem(m.Kind.ToString().ToLowerInvariant(), m.Url)).ToList(),
            myVote, isFavorite,
            user is not null && (current.IsAdmin || publisher.UserId == user.Id));
    }
}
