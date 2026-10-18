using OpenArcade.Domain.Community;

namespace OpenArcade.Api.Features.Comments.V1;

/// <summary>A comment or reply as the comment list renders it.</summary>
public sealed record CommentResponse(
    Guid Id, Guid? ParentId, string Username, string? AvatarUrl, string Body, int LikeCount,
    bool Pinned, bool LikedByMe, bool Mine, int ReplyCount, DateTimeOffset CreatedAt)
{
    public static CommentResponse From(CommentView c) => new(
        c.Id, c.ParentId, c.Username, c.AvatarUrl, c.Body, c.LikeCount, c.Pinned, c.LikedByViewer, c.ByViewer, c.ReplyCount, c.CreatedAt);
}
