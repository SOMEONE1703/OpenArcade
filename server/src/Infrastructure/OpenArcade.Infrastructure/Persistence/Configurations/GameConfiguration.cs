using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<GameRecord>
{
    public void Configure(EntityTypeBuilder<GameRecord> e)
    {
        e.HasIndex(x => x.Slug).IsUnique();
        e.HasIndex(x => x.Status);
        e.HasIndex(x => x.Tags).HasMethod("gin");
        e.HasMany(x => x.Media).WithOne().HasForeignKey(m => m.GameId);
    }
}
