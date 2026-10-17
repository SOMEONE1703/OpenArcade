using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;

namespace OpenArcade.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me", GetMe).RequireAuthorization().WithTags("Account");
        app.MapPost("/api/me/publisher-request", ApplyAsPublisher).RequireAuthorization().WithTags("Account");
        app.MapGet("/api/users/{username}", GetProfile).WithTags("Account");
    }

    private static async Task<MeDto> GetMe(AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        var roles = current.Principal.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Where(r => r is Roles.Player or Roles.Publisher or Roles.Admin).Distinct().ToList();
        var isPublisher = await db.Publishers.AnyAsync(p => p.UserId == user.Id && p.Status == PublisherStatus.Active, ct);
        var request = await db.PublisherRequests.Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt).Select(r => (RequestStatus?)r.Status).FirstOrDefaultAsync(ct);

        return new MeDto(user.Id, user.Username, user.AvatarUrl, roles, isPublisher, request?.ToString());
    }

    private static async Task<IResult> ApplyAsPublisher(
        PublisherApplication body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        if (string.IsNullOrWhiteSpace(body.StudioName)) return Results.BadRequest("Studio name is required.");
        if (await db.Publishers.AnyAsync(p => p.UserId == user.Id, ct)) return Results.Conflict("You are already a publisher.");
        if (await db.PublisherRequests.AnyAsync(r => r.UserId == user.Id && r.Status == RequestStatus.Pending, ct))
            return Results.Conflict("You already have a pending request.");

        db.PublisherRequests.Add(new PublisherRequest
        {
            UserId = user.Id, StudioName = body.StudioName.Trim(), Links = body.Links ?? [],
        });
        await db.SaveChangesAsync(ct);
        return Results.Accepted();
    }

    private static async Task<IResult> GetProfile(string username, AppDbContext db, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
        if (user is null) return Results.NotFound();

        var favorites = await db.Favorites.Where(f => f.UserId == user.Id && f.Game.Status == GameStatus.Listed)
            .OrderByDescending(f => f.CreatedAt).Select(f => f.Game).ToSummaries().ToListAsync(ct);

        return Results.Ok(new ProfileDto(user.Username, user.AvatarUrl, user.CreatedAt, favorites));
    }
}
