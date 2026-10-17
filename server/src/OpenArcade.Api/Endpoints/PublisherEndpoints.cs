using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OpenArcade.Api.Auth;
using OpenArcade.Api.Data;
using OpenArcade.Api.Services;

namespace OpenArcade.Api.Endpoints;

/// <summary>
/// Publisher flow: register a game, verify the domain, confirm it allows embedding, submit for review.
/// </summary>
public static partial class PublisherEndpoints
{
    public static void MapPublisherEndpoints(this IEndpointRouteBuilder app)
    {
        var pub = app.MapGroup("/api/publisher/games").WithTags("Publisher").RequireAuthorization();

        pub.MapGet("/", ListMine);
        pub.MapPost("/", Create);
        pub.MapPut("/{id:guid}", Update);
        pub.MapPost("/{id:guid}/verify-domain", VerifyDomain);
        pub.MapPost("/{id:guid}/check-embed", CheckEmbed);
        pub.MapPost("/{id:guid}/submit", Submit);
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    private static async Task<Publisher?> MyPublisherAsync(AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var user = await current.RequireAsync(ct);
        return await db.Publishers.FirstOrDefaultAsync(p => p.UserId == user.Id && p.Status == PublisherStatus.Active, ct);
    }

    private static async Task<IResult> ListMine(AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();

        var games = await db.Games.Where(g => g.PublisherId == publisher.Id).OrderBy(g => g.Title).ToListAsync(ct);
        return Results.Ok(games.Select(g => g.ToPublisherDto()));
    }

    private static async Task<IResult> Create(UpsertGameRequest body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();
        if (Validate(body) is { } problem) return problem;
        if (await db.Games.AnyAsync(g => g.Slug == body.Slug, ct)) return Results.Conflict("That slug is taken.");

        var game = new Game
        {
            PublisherId = publisher.Id,
            VerificationToken = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16)),
        };
        Apply(game, body);
        db.Games.Add(game);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/publisher/games/{game.Id}", game.ToPublisherDto());
    }

    private static async Task<IResult> Update(Guid id, UpsertGameRequest body, AppDbContext db, CurrentUser current, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();
        if (Validate(body) is { } problem) return problem;

        var game = await db.Games.Include(g => g.Media).FirstOrDefaultAsync(g => g.Id == id && g.PublisherId == publisher.Id, ct);
        if (game is null) return Results.NotFound();
        if (game.Slug != body.Slug && await db.Games.AnyAsync(g => g.Slug == body.Slug, ct)) return Results.Conflict("That slug is taken.");

        var oldUrl = game.PlayUrl;
        var oldDomain = game.Domain;
        db.GameMedia.RemoveRange(game.Media);
        Apply(game, body);

        // Changing the play URL or domain sends the game back to review.
        if (game.PlayUrl != oldUrl || game.Domain != oldDomain)
        {
            if (game.Domain != oldDomain) game.DomainVerifiedAt = null;
            if (game.Status is GameStatus.Listed or GameStatus.Hidden) game.Status = GameStatus.InReview;
        }

        await db.SaveChangesAsync(ct);
        return Results.Ok(game.ToPublisherDto());
    }

    private static async Task<IResult> VerifyDomain(Guid id, AppDbContext db, CurrentUser current, DomainVerifier verifier, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id && g.PublisherId == publisher.Id, ct);
        if (game is null) return Results.NotFound();

        var ok = await verifier.VerifyAsync(game.PlayUrl, game.VerificationToken, ct);
        db.GameChecks.Add(new GameCheck { GameId = game.Id, Kind = CheckKind.Domain, Passed = ok, Result = ok ? "verified" : "token not found" });
        if (ok) game.DomainVerifiedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            verified = ok,
            instructions = $"Add a DNS TXT record \"openarcade-verify={game.VerificationToken}\" on {game.Domain}, " +
                           $"or serve {game.VerificationToken} at https://{game.Domain}/.well-known/openarcade.txt",
        });
    }

    private static async Task<IResult> CheckEmbed(Guid id, AppDbContext db, CurrentUser current, EmbedChecker checker, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id && g.PublisherId == publisher.Id, ct);
        if (game is null) return Results.NotFound();

        var result = await checker.CheckAsync(game.PlayUrl, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Submit(Guid id, AppDbContext db, CurrentUser current, EmbedChecker checker, CancellationToken ct)
    {
        var publisher = await MyPublisherAsync(db, current, ct);
        if (publisher is null) return Results.Forbid();
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id && g.PublisherId == publisher.Id, ct);
        if (game is null) return Results.NotFound();
        if (game.Status is not (GameStatus.Draft or GameStatus.Hidden)) return Results.Conflict($"The game is {game.Status}.");
        if (game.DomainVerifiedAt is null) return Results.BadRequest("Verify the domain first.");

        var embed = await checker.CheckAsync(game.PlayUrl, ct);
        if (!embed.Allowed) return Results.BadRequest(embed.Detail);

        game.Status = GameStatus.InReview;
        await db.SaveChangesAsync(ct);
        return Results.Ok(game.ToPublisherDto());
    }

    private static IResult? Validate(UpsertGameRequest body)
    {
        if (!SlugPattern().IsMatch(body.Slug ?? "")) return Results.BadRequest("Slug must be lowercase letters, numbers and dashes.");
        if (string.IsNullOrWhiteSpace(body.Title)) return Results.BadRequest("Title is required.");
        if (!Uri.TryCreate(body.PlayUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http"))
            return Results.BadRequest("Play URL must be an absolute http(s) URL.");
        return null;
    }

    private static void Apply(Game game, UpsertGameRequest body)
    {
        game.Slug = body.Slug;
        game.Title = body.Title.Trim();
        game.Description = body.Description?.Trim() ?? "";
        game.Tags = (body.Tags ?? []).Select(t => t.Trim().ToLowerInvariant()).Where(t => t.Length > 0).Distinct().ToList();
        game.PlayUrl = body.PlayUrl;
        game.Domain = new Uri(body.PlayUrl).Host;
        game.MobileFriendly = body.MobileFriendly;

        var media = new List<GameMedia>();
        if (!string.IsNullOrWhiteSpace(body.ThumbnailUrl))
            media.Add(new GameMedia { Kind = MediaKind.Thumbnail, Url = body.ThumbnailUrl });
        media.AddRange((body.Screenshots ?? []).Select((url, i) => new GameMedia { Kind = MediaKind.Screenshot, Url = url, SortOrder = i + 1 }));
        game.Media = media;
    }
}
