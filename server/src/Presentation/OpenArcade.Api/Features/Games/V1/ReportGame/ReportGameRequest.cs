namespace OpenArcade.Api.Features.Games.V1.ReportGame;

/// <param name="Reason">For example Broken, Inappropriate, Not as described, or Phishing or scam.</param>
public sealed record ReportGameRequest(string Reason);
