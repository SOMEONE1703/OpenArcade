using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;
using OpenArcade.Api.Services;

namespace OpenArcade.Api.Endpoints;

/// <summary>
/// Instagram-style comments: top-level list (new or top), one level of replies, likes, @mentions in the body,
/// authors delete their own, publishers pin one and hide (collapse) others on their own game.
/// </summary>
public static class CommentEndpoints
{
    public const string PostRateLimit = "comment-post";

    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/games/{slug}/comments", ListComments).WithTags("Comments");
        app.MapPost("/api/games/{slug}/comments", PostComment).WithTags("Comments")
            .RequireAuthorization().RequireRateLimiting(PostRateLimit);

        var comments = app.MapGroup("/api/comments/{id:guid}").WithTags("Comments");
        comments.MapGet("/replies", ListReplies);
        comments.MapDelete("/", DeleteComment).RequireAuthorization();
        comments.MapPut("/like", Like).RequireAuthorization();
        comments.MapDelete("/like", Unlike).RequireAuthorization();
        comments.MapPost("/pin", Pin).RequireAuthorization();
        comments.MapPost("/hide", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) => SetHidden(id, true, db, cu, ct)).RequireAuthorization();
        comments.MapPost("/unhide", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) => SetHidden(id, false, db, cu, ct)).RequireAuthorization();
        comments.MapPost("/reports", Report).RequireAuthorization();
    }

    private static async Task<IResult> ListComments(
        string slug, string? sort, int? skip, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var gameId = await db.Games.Where(g => g.Slug == slug && g.Status == GameStatus.Listed)
            .Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();

        var topLevel = db.Comments.Where(c => c.GameId == gameId && c.ParentId == null);
        var visible = topLevel.Where(c => c.Status == CommentStatus.Visible);

        IOrderedQueryable<Comment> ordered = visible.OrderByDescending(c => c.Pinned);
        ordered = sort == "top"
            ? ordered.ThenByDescending(c => c.LikeCount).ThenByDescending(c => c.CreatedAt)
            : ordered.ThenByDescending(c => c.CreatedAt);

        var user = await current.GetAsync(ct);
        var items = await Project(ordered.Skip(skip ?? 0).Take(30), db, user?.Id).ToListAsync(ct);
        var hidden = await topLevel.CountAsync(c => c.Status == CommentStatus.Hidden, ct);
        return Results.Ok(new CommentPage(items, hidden));
    }

    private static async Task<IResult> ListReplies(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.GetAsync(ct);
        var replies = db.Comments.Where(c => c.ParentId == id && c.Status == CommentStatus.Visible)
            .OrderBy(c => c.CreatedAt);
        return Results.Ok(await Project(replies, db, user?.Id).ToListAsync(ct));
    }

    private static IQueryable<CommentDto> Project(IQueryable<Comment> q, AppDbContext db, Guid? me) =>
        q.Select(c => new CommentDto(
            c.Id, c.ParentId, c.User.Username, c.User.AvatarUrl, c.Body, c.LikeCount, c.Pinned,
            me != null && db.CommentLikes.Any(l => l.CommentId == c.Id && l.UserId == me),
            me != null && c.UserId == me,
            db.Comments.Count(r => r.ParentId == c.Id && r.Status == CommentStatus.Visible),
            c.CreatedAt));

    private static async Task<IResult> PostComment(
        string slug, NewCommentRequest body, AppDbContext db, CurrentUser current, ProfanityFilter filter, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (user.Status == UserStatus.Banned) return Results.Forbid();

        var text = body.Body?.Trim() ?? "";
        if (text.Length is 0 or > 2000) return Results.BadRequest("Comments must be 1 to 2000 characters.");
        if (!filter.IsClean(text)) return Results.BadRequest("Your comment contains blocked words.");

        var gameId = await db.Games.Where(g => g.Slug == slug && g.Status == GameStatus.Listed)
            .Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        if (gameId is null) return Results.NotFound();

        if (body.ParentId is { } parentId)
        {
            var parent = await db.Comments.FirstOrDefaultAsync(c => c.Id == parentId && c.GameId == gameId, ct);
            if (parent is null) return Results.BadRequest("Parent comment not found.");
            // Replies are one level deep: replying to a reply attaches to its parent.
            body = body with { ParentId = parent.ParentId ?? parent.Id };
        }

        var comment = new Comment { GameId = gameId.Value, UserId = user.Id, ParentId = body.ParentId, Body = text };
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/comments/{comment.Id}", new CommentDto(
            comment.Id, comment.ParentId, user.Username, user.AvatarUrl, comment.Body, 0, false, false, true, 0, comment.CreatedAt));
    }

    private static async Task<IResult> DeleteComment(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var c = await db.Comments.FindAsync([id], ct);
        if (c is null) return Results.NotFound();
        if (c.UserId != user.Id && !current.IsAdmin) return Results.Forbid();

        c.Status = CommentStatus.Deleted;
        c.Pinned = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Like(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (user.Status == UserStatus.Banned) return Results.Forbid();
        if (!await db.Comments.AnyAsync(c => c.Id == id && c.Status == CommentStatus.Visible, ct)) return Results.NotFound();

        if (!await db.CommentLikes.AnyAsync(l => l.UserId == user.Id && l.CommentId == id, ct))
        {
            db.CommentLikes.Add(new CommentLike { UserId = user.Id, CommentId = id });
            await db.SaveChangesAsync(ct);
            await db.Comments.Where(c => c.Id == id).ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount + 1), ct);
        }
        return Results.NoContent();
    }

    private static async Task<IResult> Unlike(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var removed = await db.CommentLikes.Where(l => l.UserId == user.Id && l.CommentId == id).ExecuteDeleteAsync(ct);
        if (removed > 0)
            await db.Comments.Where(c => c.Id == id).ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount - 1), ct);
        return Results.NoContent();
    }

    /// <summary>Publisher of the game (or an admin) pins one top-level comment; pinning again unpins.</summary>
    private static async Task<IResult> Pin(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var (comment, error) = await LoadForModerationAsync(id, db, current, ct);
        if (error is not null) return error;
        if (comment!.ParentId is not null) return Results.BadRequest("Only top-level comments can be pinned.");

        var pin = !comment.Pinned;
        await db.Comments.Where(c => c.GameId == comment.GameId && c.Pinned)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Pinned, false), ct);
        comment.Pinned = pin;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    /// <summary>Hiding only collapses a comment behind a count; only admins remove comments.</summary>
    private static async Task<IResult> SetHidden(Guid id, bool hidden, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var (comment, error) = await LoadForModerationAsync(id, db, current, ct);
        if (error is not null) return error;
        if (comment!.Status == CommentStatus.Deleted) return Results.NotFound();

        comment.Status = hidden ? CommentStatus.Hidden : CommentStatus.Visible;
        if (hidden) comment.Pinned = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<(Comment?, IResult?)> LoadForModerationAsync(Guid id, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var comment = await db.Comments.FindAsync([id], ct);
        if (comment is null) return (null, Results.NotFound());

        var ownsGame = await db.Games.AnyAsync(g => g.Id == comment.GameId && g.Publisher.UserId == user.Id, ct);
        return ownsGame || current.IsAdmin ? (comment, null) : (null, Results.Forbid());
    }

    private static async Task<IResult> Report(Guid id, ReportRequest body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (!await db.Comments.AnyAsync(c => c.Id == id, ct)) return Results.NotFound();
        if (string.IsNullOrWhiteSpace(body.Reason)) return Results.BadRequest("A reason is required.");

        db.Reports.Add(new Report
        {
            ReporterId = user.Id, TargetType = ReportTarget.Comment, TargetId = id,
            Reason = body.Reason.Trim()[..Math.Min(body.Reason.Trim().Length, 500)],
        });
        await db.SaveChangesAsync(ct);
        return Results.Accepted();
    }
}
