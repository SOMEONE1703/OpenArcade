namespace OpenArcade.Domain.Games;

/// <summary>One play: a game, a visitor (or signed-in user) and a day. Counted at most once per day.</summary>
public sealed record Play(Guid GameId, string VisitorId, Guid? UserId, DateOnly Day);
