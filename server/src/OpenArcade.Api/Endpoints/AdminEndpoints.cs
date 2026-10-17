using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;

namespace OpenArcade.Api.Endpoints;

/// <summary>Admin queue: publisher requests, game review, reports, bans and the delist kill switch.</summary>
public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization(Roles.Admin);

        admin.MapGet("/publisher-requests", async (AppDbContext db, CancellationToken ct) =>
            await db.PublisherRequests.Where(r => r.Status == RequestStatus.Pending).OrderBy(r => r.CreatedAt)
                .Select(r => new { r.Id, r.User.Username, r.StudioName, r.Links, r.CreatedAt }).ToListAsync(ct));

        admin.MapPost("/publisher-requests/{id:guid}/approve", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) =>
            ReviewRequest(id, true, db, cu, ct));
        admin.MapPost("/publisher-requests/{id:guid}/reject", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) =>
            ReviewRequest(id, false, db, cu, ct));

        admin.MapGet("/review-queue", async (AppDbContext db, CancellationToken ct) =>
            await db.Games.Where(g => g.Status == GameStatus.InReview).OrderBy(g => g.CreatedAt)
                .Select(g => new { g.Id, g.Slug, g.Title, g.PlayUrl, g.Domain, Studio = g.Publisher.StudioName, g.DomainVerifiedAt })
                .ToListAsync(ct));

        admin.MapPost("/games/{id:guid}/approve", (Guid id, AppDbContext db, CancellationToken ct) =>
            SetGameStatus(id, GameStatus.Listed, db, ct, from: [GameStatus.InReview, GameStatus.Hidden]));
        admin.MapPost("/games/{id:guid}/reject", (Guid id, AppDbContext db, CancellationToken ct) =>
            SetGameStatus(id, GameStatus.Draft, db, ct, from: [GameStatus.InReview]));
        // Kill switch: removes the play page immediately.
        admin.MapPost("/games/{id:guid}/delist", (Guid id, AppDbContext db, CancellationToken ct) =>
            SetGameStatus(id, GameStatus.Delisted, db, ct, from: null));

        admin.MapGet("/reports", async (AppDbContext db, CancellationToken ct) =>
            await db.Reports.Where(r => r.Status == ReportStatus.Open).OrderBy(r => r.CreatedAt).Take(100).ToListAsync(ct));

        admin.MapPost("/reports/{id:guid}/resolve", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) =>
            CloseReport(id, ReportStatus.Resolved, db, cu, ct));
        admin.MapPost("/reports/{id:guid}/dismiss", (Guid id, AppDbContext db, CurrentUser cu, CancellationToken ct) =>
            CloseReport(id, ReportStatus.Dismissed, db, cu, ct));

        admin.MapPost("/users/{id:guid}/ban", (Guid id, AppDbContext db, CancellationToken ct) => SetUserStatus(id, UserStatus.Banned, db, ct));
        admin.MapPost("/users/{id:guid}/unban", (Guid id, AppDbContext db, CancellationToken ct) => SetUserStatus(id, UserStatus.Active, db, ct));
    }

    private static async Task<IResult> ReviewRequest(Guid id, bool approve, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var admin = await current.RequireAsync(ct);
        var request = await db.PublisherRequests.FirstOrDefaultAsync(r => r.Id == id && r.Status == RequestStatus.Pending, ct);
        if (request is null) return Results.NotFound();

        request.Status = approve ? RequestStatus.Approved : RequestStatus.Rejected;
        request.ReviewedBy = admin.Id;
        // TODO: also grant the "publisher" realm role in Keycloak via its admin API. For now the
        // publishers row is what the publisher endpoints check.
        if (approve) db.Publishers.Add(new Publisher { UserId = request.UserId, StudioName = request.StudioName });

        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetGameStatus(Guid id, GameStatus to, AppDbContext db, CancellationToken ct, GameStatus[]? from)
    {
        var game = await db.Games.FindAsync([id], ct);
        if (game is null) return Results.NotFound();
        if (from is not null && !from.Contains(game.Status)) return Results.Conflict($"The game is {game.Status}.");

        game.Status = to;
        if (to == GameStatus.Listed) game.ConsecutiveHealthFailures = 0;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CloseReport(Guid id, ReportStatus status, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var admin = await current.RequireAsync(ct);
        var report = await db.Reports.FindAsync([id], ct);
        if (report is null) return Results.NotFound();

        report.Status = status;
        report.HandledBy = admin.Id;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetUserStatus(Guid id, UserStatus status, AppDbContext db, CancellationToken ct)
    {
        var updated = await db.Users.Where(u => u.Id == id).ExecuteUpdateAsync(s => s.SetProperty(u => u.Status, status), ct);
        return updated == 0 ? Results.NotFound() : Results.NoContent();
    }
}
