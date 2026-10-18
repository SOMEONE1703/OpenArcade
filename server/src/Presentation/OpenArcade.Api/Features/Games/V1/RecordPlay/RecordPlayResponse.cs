namespace OpenArcade.Api.Features.Games.V1.RecordPlay;

/// <param name="Counted">False when this visitor already played the game today.</param>
public sealed record RecordPlayResponse(bool Counted);
