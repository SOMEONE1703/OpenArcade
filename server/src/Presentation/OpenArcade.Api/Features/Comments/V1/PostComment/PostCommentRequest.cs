namespace OpenArcade.Api.Features.Comments.V1.PostComment;

/// <param name="ParentId">The comment being replied to. Replies are one level deep.</param>
public sealed record PostCommentRequest(string Body, Guid? ParentId);
