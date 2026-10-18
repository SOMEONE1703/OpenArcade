using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;

namespace OpenArcade.Api.Features.Comments.V1.PinComment;

public sealed class PinCommentHandler(
    ICommentRepository comments, IGameRepository games, IPublisherRepository publishers, IUnitOfWork uow, CurrentUser current)
{
    /// <summary>Pins a top-level comment, unpinning any other on the game. Pinning a pinned comment unpins it.</summary>
    public async Task<Result> HandleAsync(Guid id, CancellationToken ct)
    {
        var loaded = await CommentModeration.LoadAsync(id, comments, games, publishers, current, ct);
        if (loaded.Error is { } error) return error;
        var comment = loaded.Value!;

        var pin = !comment.Pinned;
        comment.SetPinned(pin);
        if (pin) await comments.UnpinAllAsync(comment.GameId, ct);
        comments.Update(comment);
        await uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
