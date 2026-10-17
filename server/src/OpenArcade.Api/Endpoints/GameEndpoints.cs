using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;
using OpenArcade.Api.Services;

namespace OpenArcade.Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var games = app.MapGroup("/api/games").WithTags("Games");

        games.MapGet("/", ListGames);
        games.MapGet("/tags", ListTags);
        games.MapGet("/{slug}", GetGame);
        games.MapPost("/{slug}/plays", RecordPlay);

        games.MapPut("/{slug}/vote", SetVote).RequireAuthorization();
        games.MapDelete("/{slug}/vote", ClearVote).RequireAuthorization();
        games.MapPut("/{slug}/favorite", AddFavorite).RequireAuthorization();
        games.MapDelete("/{slug}/favorite", RemoveFavorite).RequireAuthorization();
        games.MapPost("/{slug}/reports", ReportGame).RequireAuthorization();
    }

    private static IQueryable<Game> Listed(AppDbContext db) => db.Games.Where(g => g.Status == GameStatus.Listed);

    /// <param name="sort">most-played, most-played-30d, trending, top, or new.</param>
    private static async Task<List<GameSummary>> ListGames(
        AppDbContext db, string? sort, string? tag, string? q, int? limit, CancellationToken ct)
    {
        var query = Listed(db);
        if (!string.IsNullOrWhiteSpace(tag)) query = query.Where(g => g.Tags.Contains(tag));
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(g => EF.Functions.ILike(g.Title, $"%{q}%") || EF.Functions.ILike(g.Description, $"%{q}%"));

        var take = Math.Clamp(limit ?? 24, 1, 100);

        if (sort == "top")
        {
            // Wilson lower bound isn't translatable to SQL here, so rank a bounded candidate set in memory.
            var candidates = await query.Where(g => g.LikeCount + g.DislikeCount > 0)
                .OrderByDescending(g => g.LikeCount).Take(500).ToSummaries().ToListAsync(ct);
            return candidates.OrderByDescending(g => Ranking.WilsonLowerBound(g.LikeCount, g.DislikeCount))
                .Take(take).ToList();
        }

        query = sort switch
        {
            "most-played" => query.OrderByDescending(g => g.PlayCount),
            "most-played-30d" => query.OrderByDescending(g => g.PlayCount30d),
            "trending" => query.OrderByDescending(g => g.TrendingScore),
            _ => query.OrderByDescending(g => g.CreatedAt),
        };
        return await query.Take(take).ToSummaries().ToListAsync(ct);
    }

    private static async Task<List<string>> ListTags(AppDbContext db, CancellationToken ct) =>
        await Listed(db).SelectMany(g => g.Tags).Distinct().OrderBy(t => t).ToListAsync(ct);

    private static async Task<IResult> GetGame(string slug, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var g = await Listed(db).Include(x => x.Publisher).Include(x => x.Media)
            .FirstOrDefaultAsync(x => x.Slug == slug, ct);
        if (g is null) return Results.NotFound();

        var user = await current.GetAsync(ct);
        string? myVote = null;
        var isFavorite = false;
        if (user is not null)
        {
            var vote = await db.Votes.FirstOrDefaultAsync(v => v.UserId == user.Id && v.GameId == g.Id, ct);
            myVote = vote is null ? null : vote.Value == VoteValue.Like ? "like" : "dislike";
            isFavorite = await db.Favorites.AnyAsync(f => f.UserId == user.Id && f.GameId == g.Id, ct);
        }

        return Results.Ok(new GameDetail(
            g.Id, g.Slug, g.Title, g.Description, g.Tags, g.Publisher.StudioName, g.PlayUrl, g.Domain, g.MobileFriendly,
            g.LikeCount, g.DislikeCount, Mapping.LikePercent(g.LikeCount, g.DislikeCount), g.PlayCount,
            g.Media.OrderBy(m => m.SortOrder).Select(m => new MediaDto(m.Kind.ToString().ToLowerInvariant(), m.Url)).ToList(),
            myVote, isFavorite, user is not null && (current.IsAdmin || g.Publisher.UserId == user.Id)));
    }

    /// <summary>
    /// Called by the play page when the game's iframe finishes loading. Counted at most once per
    /// visitor (or signed-in user) per game per day.
    /// </summary>
    private static async Task<IResult> RecordPlay(
        string slug, HttpRequest req, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var gameId = await Listed(db).Where(g => g.Slug == slug).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();

        var user = await current.GetAsync(ct);
        var visitorId = user is not null ? $"u:{user.Id}" : req.Headers["X-Visitor-Id"].ToString();
        if (string.IsNullOrWhiteSpace(visitorId) || visitorId.Length > 64) return Results.BadRequest("Missing X-Visitor-Id.");

        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO plays (game_id, visitor_id, user_id, day)
            VALUES ({gameId}, {visitorId}, {user?.Id}, {day})
            ON CONFLICT DO NOTHING
            """, ct);

        if (inserted > 0)
            await db.Games.Where(g => g.Id == gameId)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.PlayCount, g => g.PlayCount + 1), ct);

        return Results.Ok(new { counted = inserted > 0 });
    }

    private static async Task<IResult> SetVote(
        string slug, VoteRequest body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (user.Status == UserStatus.Banned) return Results.Forbid();
        if (body.Value is not ("like" or "dislike")) return Results.BadRequest("Value must be like or dislike.");

        var game = await Listed(db).Include(g => g.Publisher).FirstOrDefaultAsync(g => g.Slug == slug, ct);
        if (game is null) return Results.NotFound();
        if (game.Publisher.UserId == user.Id) return Results.BadRequest("You can't vote on your own game.");

        var value = body.Value == "like" ? VoteValue.Like : VoteValue.Dislike;
        var vote = await db.Votes.FindAsync([user.Id, game.Id], ct);
        if (vote is null) db.Votes.Add(new Vote { UserId = user.Id, GameId = game.Id, Value = value });
        else { vote.Value = value; vote.UpdatedAt = DateTimeOffset.UtcNow; }
        await db.SaveChangesAsync(ct);

        return Results.Ok(await RefreshVoteCountsAsync(db, game.Id, ct));
    }

    private static async Task<IResult> ClearVote(string slug, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var gameId = await Listed(db).Where(g => g.Slug == slug).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();

        await db.Votes.Where(v => v.UserId == user.Id && v.GameId == gameId).ExecuteDeleteAsync(ct);
        return Results.Ok(await RefreshVoteCountsAsync(db, gameId.Value, ct));
    }

    private static async Task<object> RefreshVoteCountsAsync(AppDbContext db, Guid gameId, CancellationToken ct)
    {
        var likes = await db.Votes.CountAsync(v => v.GameId == gameId && v.Value == VoteValue.Like, ct);
        var dislikes = await db.Votes.CountAsync(v => v.GameId == gameId && v.Value == VoteValue.Dislike, ct);
        await db.Games.Where(g => g.Id == gameId).ExecuteUpdateAsync(s => s
            .SetProperty(g => g.LikeCount, likes)
            .SetProperty(g => g.DislikeCount, dislikes), ct);
        return new { likeCount = likes, dislikeCount = dislikes, likePercent = Mapping.LikePercent(likes, dislikes) };
    }

    private static async Task<IResult> AddFavorite(string slug, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var gameId = await Listed(db).Where(g => g.Slug == slug).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();

        if (!await db.Favorites.AnyAsync(f => f.UserId == user.Id && f.GameId == gameId, ct))
        {
            db.Favorites.Add(new Favorite { UserId = user.Id, GameId = gameId.Value });
            await db.SaveChangesAsync(ct);
        }
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFavorite(string slug, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        await db.Favorites.Where(f => f.UserId == user.Id && f.Game.Slug == slug).ExecuteDeleteAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportGame(
        string slug, ReportRequest body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var gameId = await db.Games.Where(g => g.Slug == slug).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();
        if (string.IsNullOrWhiteSpace(body.Reason)) return Results.BadRequest("A reason is required.");

        db.Reports.Add(new Report
        {
            ReporterId = user.Id, TargetType = ReportTarget.Game, TargetId = gameId.Value,
            Reason = body.Reason.Trim()[..Math.Min(body.Reason.Trim().Length, 500)],
        });
        await db.SaveChangesAsync(ct);
        return Results.Accepted();
    }
}
