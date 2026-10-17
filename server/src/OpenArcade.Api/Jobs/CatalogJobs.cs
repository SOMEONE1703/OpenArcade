using Hangfire;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Data;
using OpenArcade.Api.Services;

namespace OpenArcade.Api.Jobs;

public class CatalogJobs(AppDbContext db, EmbedChecker embed, DomainVerifier domains, ILogger<CatalogJobs> log)
{
    public static void Schedule(IRecurringJobManager jobs)
    {
        jobs.AddOrUpdate<CatalogJobs>("refresh-stats", j => j.RefreshStatsAsync(CancellationToken.None), Cron.Hourly);
        jobs.AddOrUpdate<CatalogJobs>("health-check", j => j.HealthCheckAsync(CancellationToken.None), Cron.Daily);
        jobs.AddOrUpdate<CatalogJobs>("domain-reverify", j => j.ReverifyDomainsAsync(CancellationToken.None), Cron.Monthly);
        // Weekly change detection (screenshot + script hashes) needs Playwright and is not built yet.
    }

    /// <summary>Recomputes cached counts and the trending-this-week score for every listed game.</summary>
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task RefreshStatsAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
        var since7d = DateTimeOffset.UtcNow.AddDays(-7);

        var games = await db.Games.Where(g => g.Status == GameStatus.Listed).ToListAsync(ct);
        foreach (var g in games)
        {
            var plays = db.Plays.Where(p => p.GameId == g.Id);
            g.PlayCount = await plays.CountAsync(ct);
            g.PlayCount30d = await plays.CountAsync(p => p.Day > monthAgo, ct);
            g.LikeCount = await db.Votes.CountAsync(v => v.GameId == g.Id && v.Value == VoteValue.Like, ct);
            g.DislikeCount = await db.Votes.CountAsync(v => v.GameId == g.Id && v.Value == VoteValue.Dislike, ct);

            var plays7d = await plays.CountAsync(p => p.Day > weekAgo, ct);
            var likes7d = await db.Votes.CountAsync(v => v.GameId == g.Id && v.Value == VoteValue.Like && v.UpdatedAt > since7d, ct);
            var comments7d = await db.Comments.CountAsync(c => c.GameId == g.Id && c.CreatedAt > since7d && c.Status != CommentStatus.Deleted, ct);
            var favorites7d = await db.Favorites.CountAsync(f => f.GameId == g.Id && f.CreatedAt > since7d, ct);

            // Baseline: average weekly plays over the 30 days before this week.
            var prior = await plays.CountAsync(p => p.Day > monthAgo && p.Day <= weekAgo, ct);
            var baseline = prior / (23.0 / 7);

            g.TrendingScore = Ranking.Trending(plays7d, likes7d, comments7d, favorites7d, baseline);
        }
        await db.SaveChangesAsync(ct);
        log.LogInformation("Refreshed stats for {Count} games", games.Count);
    }

    /// <summary>Daily: confirm each listed game still allows embedding. Three failures in a row hides it.</summary>
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task HealthCheckAsync(CancellationToken ct)
    {
        var games = await db.Games.Where(g => g.Status == GameStatus.Listed || g.Status == GameStatus.Hidden).ToListAsync(ct);
        foreach (var g in games)
        {
            var result = await embed.CheckAsync(g.PlayUrl, ct);
            db.GameChecks.Add(new GameCheck { GameId = g.Id, Kind = CheckKind.Health, Passed = result.Allowed, Result = result.Detail });

            if (result.Allowed)
            {
                g.ConsecutiveHealthFailures = 0;
                // Recovered games that were hidden by health checks come back automatically.
                if (g.Status == GameStatus.Hidden && g.DomainVerifiedAt is not null) g.Status = GameStatus.Listed;
            }
            else if (++g.ConsecutiveHealthFailures >= 3 && g.Status == GameStatus.Listed)
            {
                g.Status = GameStatus.Hidden;
                // TODO: email the publisher.
                log.LogWarning("Hid {Slug} after {Failures} failed health checks: {Detail}", g.Slug, g.ConsecutiveHealthFailures, result.Detail);
            }
        }
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Monthly: hide games whose domain verification record is gone (expired or sold domains).</summary>
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task ReverifyDomainsAsync(CancellationToken ct)
    {
        var games = await db.Games.Where(g => g.Status == GameStatus.Listed).ToListAsync(ct);
        foreach (var g in games)
        {
            var ok = await domains.VerifyAsync(g.PlayUrl, g.VerificationToken, ct);
            db.GameChecks.Add(new GameCheck { GameId = g.Id, Kind = CheckKind.Domain, Passed = ok, Result = ok ? "verified" : "token not found" });
            if (!ok)
            {
                g.DomainVerifiedAt = null;
                g.Status = GameStatus.Hidden;
            }
        }
        await db.SaveChangesAsync(ct);
    }
}
