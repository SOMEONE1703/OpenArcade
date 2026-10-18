namespace OpenArcade.Api.Features.Admin.V1.ListReviewQueue;

public sealed record ListReviewQueueResponse(
    Guid Id, string Slug, string Title, string PlayUrl, string Domain, string Studio, DateTimeOffset? DomainVerifiedAt);
