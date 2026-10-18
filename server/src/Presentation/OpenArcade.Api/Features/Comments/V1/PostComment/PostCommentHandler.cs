using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;

namespace OpenArcade.Api.Features.Comments.V1.PostComment;

public sealed class PostCommentHandler(
    IGameRepository games, ICommentRepository comments, IContentFilter filter, IUnitOfWork uow,
    CurrentUser current, IClock clock)
{
    public async Task<Result<CommentResponse>> HandleAsync(string slug, PostCommentRequest request, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var game = await games.GetListedBySlugAsync(slug, ct);
        if (game is null) return Error.NotFound();
        if (!filter.IsClean(request.Body ?? "")) return Error.Invalid("Your comment contains blocked words.");

        Comment? parent = null;
        if (request.ParentId is { } parentId)
        {
            parent = await comments.GetAsync(parentId, ct);
            if (parent is null) return Error.Invalid("Parent comment not found.");
        }

        var comment = Comment.Post(user, game.Id, parent, request.Body ?? "", clock.UtcNow);
        comments.Add(comment);
        await uow.SaveChangesAsync(ct);

        return new CommentResponse(
            comment.Id, comment.ParentId, user.Username, user.AvatarUrl, comment.Body, 0, false, false, true, 0, comment.CreatedAt);
    }
}
