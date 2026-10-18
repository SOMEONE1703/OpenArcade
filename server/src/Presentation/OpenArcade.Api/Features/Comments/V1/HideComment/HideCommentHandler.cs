using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Comments.V1.HideComment;

/// <summary>Hiding only collapses a comment behind a count; only admins remove comments.</summary>
public sealed class HideCommentHandler(
    ICommentRepository comments, IGameRepository games, IPublisherRepository publishers, IUnitOfWork uow, CurrentUser current)
{
    public async Task<Result> HandleAsync(Guid id, bool hide, CancellationToken ct)
    {
        var loaded = await CommentModeration.LoadAsync(id, comments, games, publishers, current, ct);
        if (loaded.Error is { } error) return error;
        var comment = loaded.Value!;

        if (hide) comment.Hide();
        else comment.Unhide();
        comments.Update(comment);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
