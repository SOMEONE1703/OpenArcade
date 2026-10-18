namespace OpenArcade.Api.Features.Admin.V1.ListPublisherRequests;

public sealed record ListPublisherRequestsResponse(Guid Id, string Username, string StudioName, IReadOnlyList<string> Links, DateTimeOffset CreatedAt);
