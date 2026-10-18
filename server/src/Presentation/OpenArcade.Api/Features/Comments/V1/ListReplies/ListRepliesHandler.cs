using OpenArcade.Api.Common.Auth;
using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Comments.V1.ListReplies;

public sealed class ListRepliesHandler(ICommentRepository comments, CurrentUser current)
{
    /// <summary>Visible replies to a comment, oldest first.</summary>
    public async Task<IReadOnlyList<CommentResponse>> HandleAsync(Guid commentId, CancellationToken ct)
    {
        var viewer = await current.GetAsync(ct);
        var replies = await comments.ListRepliesAsync(commentId, viewer?.Id, ct);
        return replies.Select(CommentResponse.From).ToList();
    }
}
