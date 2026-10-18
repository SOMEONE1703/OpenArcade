using Hangfire;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Domain.Community;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Ranking;
using OpenArcade.Infrastructure.Persistence;

namespace OpenArcade.Infrastructure.Jobs;

public class CatalogJobs(
    AppDbContext db, IGameRepository games, IGameCheckRepository checks, IUnitOfWork uow,
    IEmbedChecker embed, IDomainVerifier domains, IClock clock, ILogger<CatalogJobs> log)
{
    public static void Schedule(IRecurringJobManager jobs)
    {
        jobs.AddOrUpdate<CatalogJobs>("refresh-stats", j => j.RefreshStatsAsync(CancellationToken.None), Cron.Hourly);
        jobs.AddOrUpdate<CatalogJobs>("health-check", j => j.HealthCheckAsync(CancellationToken.None), Cron.Daily);
        jobs.AddOrUpdate<CatalogJobs>("domain-reverify", j => j.ReverifyDomainsAsync(CancellationToken.None), Cron.Monthly);
        // Weekly change detection (screenshot + script hashes) needs Playwright and is not built yet.
    }

    /// <summary>
    /// Recomputes cached counts and the trending-this-week score for every listed game. This is pure aggregation,
    /// so it works on the database records directly.
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    public async Task RefreshStatsAsync(CancellationToken ct)
    {
        var today = clock.Today;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);
        var since7d = clock.UtcNow.AddDays(-7);

        var listed = await db.Games.Where(g => g.Status == GameStatus.Listed).ToListAsync(ct);
        foreach (var g in listed)
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

            // Baseline: average weekly plays over the 23 days before this week.
            var prior = await plays.CountAsync(p => p.Day > monthAgo && p.Day <= weekAgo, ct);
            g.TrendingScore = TrendingScore.Compute(plays7d, likes7d, comments7d, favorites7d, prior / (23.0 / 7));
        }
        await db.SaveChangesAsync(ct);
        log.LogInformation("Refreshed stats for {Count} games", listed.Count);
    }

    /// <summary>Daily: confirm each listed or hidden game still allows embedding.</summary>
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task HealthCheckAsync(CancellationToken ct)
    {
        foreach (var game in await games.ListByStatusAsync([GameStatus.Listed, GameStatus.Hidden], ct))
        {
            var result = await embed.CheckAsync(game.PlayUrl, ct);
            checks.Add(GameCheck.Record(game.Id, CheckKind.Health, result.Allowed, result.Detail, clock.UtcNow));

            if (game.RecordHealthCheck(result.Allowed))
                // TODO: email the publisher.
                log.LogWarning("Hid {Slug} after {Failures} failed health checks: {Detail}", game.Slug, game.ConsecutiveHealthFailures, result.Detail);

            games.Update(game);
        }
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Monthly: hide games whose domain verification record is gone (expired or sold domains).</summary>
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public async Task ReverifyDomainsAsync(CancellationToken ct)
    {
        foreach (var game in await games.ListByStatusAsync([GameStatus.Listed], ct))
        {
            var ok = await domains.VerifyAsync(game.PlayUrl, game.VerificationToken, ct);
            checks.Add(GameCheck.Record(game.Id, CheckKind.Domain, ok, ok ? "verified" : "token not found", clock.UtcNow));
            if (!ok)
            {
                game.LoseDomainVerification();
                games.Update(game);
            }
        }
        await uow.SaveChangesAsync(ct);
    }
}
