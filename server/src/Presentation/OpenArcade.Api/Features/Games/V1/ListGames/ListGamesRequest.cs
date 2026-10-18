namespace OpenArcade.Api.Features.Games.V1.ListGames;

/// <param name="Sort">trending, most-played, most-played-30d, top, or new (default).</param>
/// <param name="Q">Searches titles and descriptions.</param>
public sealed record ListGamesRequest(string? Sort, string? Tag, string? Q, int? Limit);
