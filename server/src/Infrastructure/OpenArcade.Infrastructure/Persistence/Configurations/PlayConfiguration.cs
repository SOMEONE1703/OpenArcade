using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class PlayConfiguration : IEntityTypeConfiguration<PlayRecord>
{
    public void Configure(EntityTypeBuilder<PlayRecord> e)
    {
        // One row per game, visitor and day, so refreshes don't inflate counts.
        e.HasKey(x => new { x.GameId, x.VisitorId, x.Day });
        e.Property(x => x.VisitorId).HasMaxLength(64);
        e.HasIndex(x => new { x.GameId, x.Day });
    }
}
