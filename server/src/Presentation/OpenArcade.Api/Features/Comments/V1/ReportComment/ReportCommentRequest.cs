namespace OpenArcade.Api.Features.Comments.V1.ReportComment;

/// <param name="Reason">For example Spam, Harassment or Inappropriate.</param>
public sealed record ReportCommentRequest(string Reason);
