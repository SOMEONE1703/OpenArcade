namespace OpenArcade.Api.Features.Games.V1.VoteOnGame;

/// <summary>The game's vote totals after the change, e.g. "92% liked, 1,240 votes".</summary>
public sealed record VoteOnGameResponse(int LikeCount, int DislikeCount, int? LikePercent);
