using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Games;
using OpenArcade.Domain.Publishers;
using OpenArcade.Domain.Users;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence;

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
        var now = DateTimeOffset.UtcNow;

        var user = new UserRecord
        {
            Id = Guid.CreateVersion7(), KeycloakId = "seed-sample-publisher", Username = "openarcade-samples",
            Status = UserStatus.Active, CreatedAt = now,
        };
        var publisher = new PublisherRecord
        {
            Id = Guid.CreateVersion7(), User = user, StudioName = "OpenArcade Samples", Status = PublisherStatus.Active,
        };

        GameRecord Sample(string slug, string title, string description, string[] tags)
        {
            var id = Guid.CreateVersion7();
            return new GameRecord
            {
                Id = id,
                Publisher = publisher,
                Slug = slug,
                Title = title,
                Description = description,
                Tags = [.. tags],
                PlayUrl = $"{origin}/{slug}/",
                Domain = new Uri(origin).Host,
                VerificationToken = "dev",
                DomainVerifiedAt = now,
                Status = GameStatus.Listed,
                MobileFriendly = true,
                CreatedAt = now,
                Media = [new GameMediaRecord { Id = Guid.CreateVersion7(), GameId = id, Kind = MediaKind.Thumbnail, Url = $"{origin}/{slug}/thumb.svg" }],
            };
        }

        db.Games.AddRange(
            Sample("snake", "Snake", "Eat, grow, don't bite your tail. Arrow keys or swipe.", ["arcade", "classic"]),
            Sample("reaction", "Reaction Test", "Click the moment the screen turns green. How fast are you?", ["casual", "reflex"]));

        await db.SaveChangesAsync();
    }
}
