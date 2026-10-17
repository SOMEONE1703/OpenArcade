namespace OpenArcade.Api.Services;

public record EmbedCheckResult(bool Allowed, string Detail);

/// <summary>
/// Checks that a play URL lets OpenArcade frame it: no X-Frame-Options DENY/SAMEORIGIN, and a CSP
/// frame-ancestors (if present) that includes our origin. A Playwright-based check can replace this later.
/// </summary>
public class EmbedChecker(HttpClient http, IConfiguration config)
{
    private string SiteOrigin => config["Site:Origin"] ?? "http://localhost:5173";

    public async Task<EmbedCheckResult> CheckAsync(string playUrl, CancellationToken ct = default)
    {
        HttpResponseMessage res;
        try
        {
            res = await http.GetAsync(playUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new(false, $"Could not load the play URL: {ex.Message}");
        }

        using (res)
        {
            if (!res.IsSuccessStatusCode)
                return new(false, $"The play URL returned HTTP {(int)res.StatusCode}.");

            if (res.Headers.TryGetValues("X-Frame-Options", out var xfo))
            {
                var value = string.Join(",", xfo).Trim().ToUpperInvariant();
                if (value is "DENY" or "SAMEORIGIN")
                    return new(false, $"X-Frame-Options is {value}, which blocks embedding.");
            }

            if (res.Headers.TryGetValues("Content-Security-Policy", out var csp))
            {
                var ancestors = string.Join(";", csp).Split(';')
                    .Select(d => d.Trim())
                    .FirstOrDefault(d => d.StartsWith("frame-ancestors", StringComparison.OrdinalIgnoreCase));

                if (ancestors is not null)
                {
                    var sources = ancestors.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();
                    var ok = sources.Contains("*") || sources.Any(s => s.TrimEnd('/') == SiteOrigin);
                    if (!ok)
                        return new(false, $"CSP frame-ancestors must include {SiteOrigin}.");
                }
            }

            return new(true, "The play URL allows embedding.");
        }
    }
}
