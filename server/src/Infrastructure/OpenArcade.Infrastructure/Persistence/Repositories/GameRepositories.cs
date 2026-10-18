using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Ranking;
using OpenArcade.Infrastructure.Persistence.Mappers;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Repositories;

internal sealed class GameRepository(AppDbContext db) : IGameRepository
{
    private IQueryable<GameRecord> WithMedia => db.Games.Include(g => g.Media);
    private IQueryable<GameRecord> Listed => db.Games.AsNoTracking().Where(g => g.Status == GameStatus.Listed);

    public async Task<Game?> GetAsync(Guid id, CancellationToken ct) =>
        (await WithMedia.FirstOrDefaultAsync(g => g.Id == id, ct))?.ToDomain();

    public async Task<Game?> GetBySlugAsync(string slug, CancellationToken ct) =>
        (await WithMedia.FirstOrDefaultAsync(g => g.Slug == slug, ct))?.ToDomain();

    public async Task<Game?> GetListedBySlugAsync(string slug, CancellationToken ct) =>
        (await WithMedia.FirstOrDefaultAsync(g => g.Slug == slug && g.Status == GameStatus.Listed, ct))?.ToDomain();

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) => db.Games.AnyAsync(g => g.Slug == slug, ct);

    public async Task<IReadOnlyList<Game>> ListByPublisherAsync(Guid publisherId, CancellationToken ct) =>
        (await WithMedia.Where(g => g.PublisherId == publisherId).OrderBy(g => g.Title).ToListAsync(ct))
            .Select(r => r.ToDomain()).ToList();

    public async Task<IReadOnlyList<Game>> ListByStatusAsync(IReadOnlyCollection<GameStatus> statuses, CancellationToken ct) =>
        (await WithMedia.Where(g => statuses.Contains(g.Status)).ToListAsync(ct)).Select(r => r.ToDomain()).ToList();

    public void Add(Game game)
    {
        var record = new GameRecord();
        game.CopyTo(record);
        record.Media = game.Media.Select(m => m.ToRecord(game.Id)).ToList();
        db.Games.Add(record);
    }

    public void Update(Game game)
    {
        var record = Tracked.Find(db.Games, r => r.Id == game.Id);
        game.CopyTo(record);

        var current = record.Media.OrderBy(m => m.SortOrder).Select(m => new GameMedia(m.Kind, m.Url, m.SortOrder));
        if (!current.SequenceEqual(game.Media))
        {
            db.GameMedia.RemoveRange(record.Media);
            record.Media = game.Media.Select(m => m.ToRecord(game.Id)).ToList();
            // Add explicitly: the new records already have ids, so EF would otherwise treat them as existing rows.
            db.GameMedia.AddRange(record.Media);
        }
    }

    public async Task<IReadOnlyList<GameCard>> ListCatalogAsync(CatalogQuery query, CancellationToken ct)
    {
        var q = Listed;
        if (!string.IsNullOrWhiteSpace(query.Tag)) q = q.Where(g => g.Tags.Contains(query.Tag));
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(g => EF.Functions.ILike(g.Title, $"%{query.Search}%") || EF.Functions.ILike(g.Description, $"%{query.Search}%"));

        if (query.Sort == CatalogSort.Top)
        {
            // The Wilson lower bound isn't translated to SQL, so rank a bounded candidate set in memory.
            var candidates = await q.Where(g => g.LikeCount + g.DislikeCount > 0)
                .OrderByDescending(g => g.LikeCount).Take(500).ToCards().ToListAsync(ct);
            return candidates.OrderByDescending(g => WilsonScore.LowerBound(g.LikeCount, g.DislikeCount))
                .Take(query.Limit).ToList();
        }

        q = query.Sort switch
        {
            CatalogSort.MostPlayed => q.OrderByDescending(g => g.PlayCount),
            CatalogSort.MostPlayed30d => q.OrderByDescending(g => g.PlayCount30d),
            CatalogSort.Trending => q.OrderByDescending(g => g.TrendingScore),
            _ => q.OrderByDescending(g => g.CreatedAt),
        };
        return await q.Take(query.Limit).ToCards().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> ListTagsAsync(CancellationToken ct) =>
        await Listed.SelectMany(g => g.Tags).Distinct().OrderBy(t => t).ToListAsync(ct);

    public async Task<IReadOnlyList<GameCard>> ListFavoritesAsync(Guid userId, CancellationToken ct) =>
        await db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId && f.Game.Status == GameStatus.Listed)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.Game).ToCards().ToListAsync(ct);

    public async Task<IReadOnlyList<ReviewQueueItem>> ListReviewQueueAsync(CancellationToken ct) =>
        await db.Games.AsNoTracking()
            .Where(g => g.Status == GameStatus.InReview)
            .OrderBy(g => g.CreatedAt)
            .Select(g => new ReviewQueueItem(g.Id, g.Slug, g.Title, g.PlayUrl, g.Domain, g.Publisher.StudioName, g.DomainVerifiedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AdminGameRow>> ListForAdminAsync(AdminGameQuery query, CancellationToken ct)
    {
        var q = db.Games.AsNoTracking();
        q = query.Status is { } status ? q.Where(g => g.Status == status) : q.Where(g => g.Status != GameStatus.Archived);
        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(g => EF.Functions.ILike(g.Title, $"%{query.Search}%") || EF.Functions.ILike(g.Slug, $"%{query.Search}%")
                || EF.Functions.ILike(g.Publisher.StudioName, $"%{query.Search}%"));

        return await q.OrderByDescending(g => g.CreatedAt).Take(query.Limit)
            .Select(g => new AdminGameRow(
                g.Id, g.Slug, g.Title, g.Media.Where(m => m.Kind == MediaKind.Thumbnail).Select(m => m.Url).FirstOrDefault(),
                g.Publisher.StudioName, g.Status, g.PlayUrl, g.Domain, g.DomainVerifiedAt != null, g.PlayCount, g.LikeCount,
                g.DislikeCount, g.ConsecutiveHealthFailures, g.CreatedAt))
            .ToListAsync(ct);
    }
}

internal sealed class PlayRepository(AppDbContext db) : IPlayRepository
{
    public async Task<bool> TryRecordAsync(Play play, CancellationToken ct)
    {
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO plays (game_id, visitor_id, user_id, day)
            VALUES ({play.GameId}, {play.VisitorId}, {play.UserId}, {play.Day})
            ON CONFLICT DO NOTHING
            """, ct);

        if (inserted == 0) return false;
        await db.Games.Where(g => g.Id == play.GameId)
            .ExecuteUpdateAsync(s => s.SetProperty(g => g.PlayCount, g => g.PlayCount + 1), ct);
        return true;
    }
}

internal sealed class GameCheckRepository(AppDbContext db) : IGameCheckRepository
{
    public void Add(GameCheck check) => db.GameChecks.Add(check.ToRecord());
}
