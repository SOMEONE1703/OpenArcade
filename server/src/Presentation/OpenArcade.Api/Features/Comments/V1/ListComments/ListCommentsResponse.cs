namespace OpenArcade.Api.Features.Comments.V1.ListComments;

/// <param name="HiddenCount">Top-level comments the publisher hid, shown as a count.</param>
public sealed record ListCommentsResponse(IReadOnlyList<CommentResponse> Items, int HiddenCount);
