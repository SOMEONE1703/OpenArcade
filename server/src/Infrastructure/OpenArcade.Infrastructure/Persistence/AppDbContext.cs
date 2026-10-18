using Microsoft.EntityFrameworkCore;
using OpenArcade.Domain.Abstractions;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    // Table names come from these property names (snake_cased), e.g. Games -> games.
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<PublisherRequestRecord> PublisherRequests => Set<PublisherRequestRecord>();
    public DbSet<PublisherRecord> Publishers => Set<PublisherRecord>();
    public DbSet<GameRecord> Games => Set<GameRecord>();
    public DbSet<GameMediaRecord> GameMedia => Set<GameMediaRecord>();
    public DbSet<GameCheckRecord> GameChecks => Set<GameCheckRecord>();
    public DbSet<PlayRecord> Plays => Set<PlayRecord>();
    public DbSet<VoteRecord> Votes => Set<VoteRecord>();
    public DbSet<FavoriteRecord> Favorites => Set<FavoriteRecord>();
    public DbSet<CommentRecord> Comments => Set<CommentRecord>();
    public DbSet<CommentLikeRecord> CommentLikes => Set<CommentLikeRecord>();
    public DbSet<ReportRecord> Reports => Set<ReportRecord>();

    Task IUnitOfWork.SaveChangesAsync(CancellationToken ct) => SaveChangesAsync(ct);

    protected override void ConfigureConventions(ModelConfigurationBuilder b)
    {
        // Store enums by name so the database stays readable.
        b.Properties<Enum>().HaveConversion<string>().HaveMaxLength(32);
    }

    protected override void OnModelCreating(ModelBuilder b) =>
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
