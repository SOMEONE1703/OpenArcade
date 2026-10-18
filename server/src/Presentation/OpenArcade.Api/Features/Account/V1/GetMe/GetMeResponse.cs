namespace OpenArcade.Api.Features.Account.V1.GetMe;

/// <param name="PublisherRequestStatus">Pending, Approved or Rejected for the latest request, or null.</param>
public sealed record GetMeResponse(
    Guid Id, string Username, string? AvatarUrl, IReadOnlyList<string> Roles, bool IsPublisher, string? PublisherRequestStatus);
