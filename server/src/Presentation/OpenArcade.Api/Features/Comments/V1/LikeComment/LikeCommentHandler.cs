using OpenArcade.Api.Common.Auth;
using OpenArcade.Api.Common.Results;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Comments.V1.LikeComment;

public sealed class LikeCommentHandler(ICommentRepository comments, CurrentUser current)
{
    public async Task<Result> LikeAsync(Guid id, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        user.EnsureCanParticipate();
        var comment = await comments.GetAsync(id, ct);
        if (comment is null || comment.Status != CommentStatus.Visible) return Error.NotFound();

        await comments.AddLikeAsync(user.Id, id, ct);
        return Result.Ok();
    }

    public async Task<Result> UnlikeAsync(Guid id, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        await comments.RemoveLikeAsync(user.Id, id, ct);
        return Result.Ok();
    }
}
