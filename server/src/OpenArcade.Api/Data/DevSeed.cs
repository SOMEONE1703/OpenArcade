using Microsoft.EntityFrameworkCore;

namespace OpenArcade.Api.Data;

/// <summary>
/// Development-only data: a sample publisher and the two games under /samples, served by the
/// "sample-games" container on a different origin (http://localhost:8090) so the iframe behaves as in production.
/// </summary>
public static class DevSeed
{
    public static async Task RunAsync(AppDbContext db, IConfiguration config)
    {
        if (await db.Games.AnyAsync()) return;

        var origin = (config["Samples:Origin"] ?? "http://localhost:8090").TrimEnd('/');

        var user = new User { KeycloakId = "seed-sample-publisher", Username = "openarcade-samples" };
        var publisher = new Publisher { User = user, StudioName = "OpenArcade Samples" };

        Game Sample(string slug, string title, string description, string[] tags) => new()
        {
            Publisher = publisher,
            Slug = slug,
            Title = title,
            Description = description,
            Tags = [.. tags],
            PlayUrl = $"{origin}/{slug}/",
            Domain = new Uri(origin).Host,
            VerificationToken = "dev",
            DomainVerifiedAt = DateTimeOffset.UtcNow,
            Status = GameStatus.Listed,
            MobileFriendly = true,
            Media = [new GameMedia { Kind = MediaKind.Thumbnail, Url = $"{origin}/{slug}/thumb.svg" }],
        };

        db.Games.AddRange(
            Sample("snake", "Snake", "Eat, grow, don't bite your tail. Arrow keys or swipe.", ["arcade", "classic"]),
            Sample("reaction", "Reaction Test", "Click the moment the screen turns green. How fast are you?", ["casual", "reflex"]));

        await db.SaveChangesAsync();
    }
}
