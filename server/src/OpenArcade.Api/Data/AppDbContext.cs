using Microsoft.EntityFrameworkCore;

namespace OpenArcade.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PublisherRequest> PublisherRequests => Set<PublisherRequest>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameMedia> GameMedia => Set<GameMedia>();
    public DbSet<GameCheck> GameChecks => Set<GameCheck>();
    public DbSet<Play> Plays => Set<Play>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLike> CommentLikes => Set<CommentLike>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // Store enums by name so the database stays readable.
        b.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.KeycloakId).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
        });

        b.Entity<Publisher>().HasIndex(x => x.UserId).IsUnique();

        b.Entity<Game>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.Tags).HasMethod("gin");
            e.HasMany(x => x.Media).WithOne().HasForeignKey(m => m.GameId);
        });

        b.Entity<GameCheck>().HasIndex(x => new { x.GameId, x.Kind, x.CheckedAt });

        b.Entity<Play>(e =>
        {
            e.HasKey(x => new { x.GameId, x.VisitorId, x.Day });
            e.HasIndex(x => new { x.GameId, x.Day });
        });

        b.Entity<Vote>(e =>
        {
            e.HasKey(x => new { x.UserId, x.GameId });
            e.HasIndex(x => new { x.GameId, x.UpdatedAt });
            // Votes are stored as +1/-1 so sums are easy.
            e.Property(x => x.Value).HasConversion<int>();
        });

        b.Entity<Favorite>().HasKey(x => new { x.UserId, x.GameId });

        b.Entity<Comment>(e =>
        {
            e.HasIndex(x => new { x.GameId, x.ParentId, x.CreatedAt });
            e.Property(x => x.Body).HasMaxLength(2000);
        });

        b.Entity<CommentLike>().HasKey(x => new { x.UserId, x.CommentId });

        b.Entity<Report>().HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
