namespace OpenArcade.Api.Features.Account.V1.ApplyAsPublisher;

/// <param name="Links">Links to the applicant's existing work, for the admin reviewing the request.</param>
public sealed record ApplyAsPublisherRequest(string StudioName, IReadOnlyList<string>? Links);
