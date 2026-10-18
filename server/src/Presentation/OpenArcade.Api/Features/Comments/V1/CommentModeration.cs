using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Comments.V1;

/// <summary>Loads a comment for pinning or hiding, which only the game's publisher and admins may do.</summary>
internal static class CommentModeration
{
    public static async Task<Result<Comment>> LoadAsync(
        Guid commentId, ICommentRepository comments, IGameRepository games, IPublisherRepository publishers,
        CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var comment = await comments.GetAsync(commentId, ct);
        if (comment is null) return Error.NotFound();
        if (current.IsAdmin) return comment;

        var game = await games.GetAsync(comment.GameId, ct);
        var publisher = game is null ? null : await publishers.GetAsync(game.PublisherId, ct);
        return publisher?.UserId == user.Id ? comment : Error.Forbidden();
    }
}
