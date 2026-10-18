using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Community;
using OpenArcade.Infrastructure.Persistence.Mappers;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Repositories;

internal sealed class VoteRepository(AppDbContext db) : IVoteRepository
{
    public async Task<Vote?> GetAsync(Guid userId, Guid gameId, CancellationToken ct) =>
        (await db.Votes.FirstOrDefaultAsync(v => v.UserId == userId && v.GameId == gameId, ct))?.ToDomain();

    public async Task<VoteCounts> CountAsync(Guid gameId, CancellationToken ct)
    {
        // Counts what will be in the database after SaveChanges, so call it after saving the vote.
        var likes = await db.Votes.CountAsync(v => v.GameId == gameId && v.Value == VoteValue.Like, ct);
        var dislikes = await db.Votes.CountAsync(v => v.GameId == gameId && v.Value == VoteValue.Dislike, ct);
        return new VoteCounts(likes, dislikes);
    }

    public void Add(Vote vote)
    {
        var record = new VoteRecord();
        vote.CopyTo(record);
        db.Votes.Add(record);
    }

    public void Update(Vote vote) => vote.CopyTo(Tracked.Find(db.Votes, r => r.UserId == vote.UserId && r.GameId == vote.GameId));

    public void Remove(Vote vote) => db.Votes.Remove(Tracked.Find(db.Votes, r => r.UserId == vote.UserId && r.GameId == vote.GameId));
}

internal sealed class FavoriteRepository(AppDbContext db) : IFavoriteRepository
{
    public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken ct) =>
        db.Favorites.AnyAsync(f => f.UserId == userId && f.GameId == gameId, ct);

    public void Add(Favorite favorite) =>
        db.Favorites.Add(new FavoriteRecord { UserId = favorite.UserId, GameId = favorite.GameId, CreatedAt = favorite.CreatedAt });

    public Task RemoveAsync(Guid userId, Guid gameId, CancellationToken ct) =>
        db.Favorites.Where(f => f.UserId == userId && f.GameId == gameId).ExecuteDeleteAsync(ct);
}

internal sealed class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<Comment?> GetAsync(Guid id, CancellationToken ct) =>
        (await db.Comments.FirstOrDefaultAsync(c => c.Id == id, ct))?.ToDomain();

    public void Add(Comment comment)
    {
        var record = new CommentRecord();
        comment.CopyTo(record);
        db.Comments.Add(record);
    }

    public void Update(Comment comment) => comment.CopyTo(Tracked.Find(db.Comments, r => r.Id == comment.Id));

    public Task UnpinAllAsync(Guid gameId, CancellationToken ct) =>
        db.Comments.Where(c => c.GameId == gameId && c.Pinned).ExecuteUpdateAsync(s => s.SetProperty(c => c.Pinned, false), ct);

    public async Task<bool> AddLikeAsync(Guid userId, Guid commentId, CancellationToken ct)
    {
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO comment_likes (user_id, comment_id) VALUES ({userId}, {commentId}) ON CONFLICT DO NOTHING", ct);
        if (inserted == 0) return false;
        await db.Comments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount + 1), ct);
        return true;
    }

    public async Task<bool> RemoveLikeAsync(Guid userId, Guid commentId, CancellationToken ct)
    {
        var removed = await db.CommentLikes.Where(l => l.UserId == userId && l.CommentId == commentId).ExecuteDeleteAsync(ct);
        if (removed == 0) return false;
        await db.Comments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount - 1), ct);
        return true;
    }

    public async Task<IReadOnlyList<CommentView>> ListTopLevelAsync(
        Guid gameId, CommentSort sort, int skip, int take, Guid? viewerId, CancellationToken ct)
    {
        var visible = db.Comments.Where(c => c.GameId == gameId && c.ParentId == null && c.Status == CommentStatus.Visible);
        var ordered = visible.OrderByDescending(c => c.Pinned);
        ordered = sort == CommentSort.Top
            ? ordered.ThenByDescending(c => c.LikeCount).ThenByDescending(c => c.CreatedAt)
            : ordered.ThenByDescending(c => c.CreatedAt);
        return await Project(ordered.Skip(skip).Take(take), viewerId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CommentView>> ListRepliesAsync(Guid parentId, Guid? viewerId, CancellationToken ct) =>
        await Project(db.Comments.Where(c => c.ParentId == parentId && c.Status == CommentStatus.Visible).OrderBy(c => c.CreatedAt), viewerId)
            .ToListAsync(ct);

    public Task<int> CountHiddenTopLevelAsync(Guid gameId, CancellationToken ct) =>
        db.Comments.CountAsync(c => c.GameId == gameId && c.ParentId == null && c.Status == CommentStatus.Hidden, ct);

    private IQueryable<CommentView> Project(IQueryable<CommentRecord> q, Guid? viewerId) =>
        q.AsNoTracking().Select(c => new CommentView(
            c.Id, c.ParentId, c.User.Username, c.User.AvatarUrl, c.Body, c.LikeCount, c.Pinned,
            viewerId != null && db.CommentLikes.Any(l => l.CommentId == c.Id && l.UserId == viewerId),
            viewerId != null && c.UserId == viewerId,
            db.Comments.Count(r => r.ParentId == c.Id && r.Status == CommentStatus.Visible),
            c.CreatedAt));
}

internal sealed class ReportRepository(AppDbContext db) : IReportRepository
{
    public async Task<Report?> GetAsync(Guid id, CancellationToken ct) =>
        (await db.Reports.FirstOrDefaultAsync(r => r.Id == id, ct))?.ToDomain();

    public async Task<IReadOnlyList<Report>> ListOpenAsync(int take, CancellationToken ct) =>
        (await db.Reports.AsNoTracking().Where(r => r.Status == ReportStatus.Open).OrderBy(r => r.CreatedAt).Take(take).ToListAsync(ct))
            .Select(r => r.ToDomain()).ToList();

    public void Add(Report report)
    {
        var record = new ReportRecord();
        report.CopyTo(record);
        db.Reports.Add(record);
    }

    public void Update(Report report) => report.CopyTo(Tracked.Find(db.Reports, r => r.Id == report.Id));
}
