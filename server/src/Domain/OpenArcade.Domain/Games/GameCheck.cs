namespace OpenArcade.Domain.Games;

public enum CheckKind { Health, Change, Domain }

/// <summary>The result of one automated check on a game (embedding health, change detection, domain).</summary>
public sealed class GameCheck
{
    public Guid Id { get; internal set; }
    public Guid GameId { get; internal set; }
    public CheckKind Kind { get; internal set; }
    public bool Passed { get; internal set; }
    public string Result { get; internal set; } = "";
    public string? ScreenshotUrl { get; internal set; }
    public DateTimeOffset CheckedAt { get; internal set; }

    internal GameCheck() { }

    public static GameCheck Record(Guid gameId, CheckKind kind, bool passed, string result, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        GameId = gameId,
        Kind = kind,
        Passed = passed,
        Result = result,
        CheckedAt = now,
    };
}
