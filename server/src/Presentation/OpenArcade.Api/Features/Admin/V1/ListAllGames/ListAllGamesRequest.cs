namespace OpenArcade.Api.Features.Admin.V1.ListAllGames;

/// <param name="Status">Draft, InReview, Listed, Hidden, Delisted or Archived. Leave it out for every game except archived ones.</param>
/// <param name="Q">Searches titles, slugs and studio names.</param>
public sealed record ListAllGamesRequest(string? Status, string? Q, int? Limit);
