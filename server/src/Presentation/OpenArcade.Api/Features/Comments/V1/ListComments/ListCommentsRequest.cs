namespace OpenArcade.Api.Features.Comments.V1.ListComments;

/// <param name="Sort">new (default) or top. Pinned comments always come first.</param>
public sealed record ListCommentsRequest(string? Sort, int? Skip);
