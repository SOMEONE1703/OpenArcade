namespace OpenArcade.Domain.Abstractions;

public record EmbedCheckResult(bool Allowed, string Detail);

/// <summary>Checks that a play URL lets OpenArcade frame it.</summary>
public interface IEmbedChecker
{
    Task<EmbedCheckResult> CheckAsync(string playUrl, CancellationToken ct = default);
}
