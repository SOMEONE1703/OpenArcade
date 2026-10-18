using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class VoteConfiguration : IEntityTypeConfiguration<VoteRecord>
{
    public void Configure(EntityTypeBuilder<VoteRecord> e)
    {
        e.HasKey(x => new { x.UserId, x.GameId });
        e.HasIndex(x => new { x.GameId, x.UpdatedAt });
        // Stored as +1/-1 so sums are easy.
        e.Property(x => x.Value).HasConversion<int>();
    }
}
