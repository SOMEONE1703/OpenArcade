using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class GameMediaConfiguration : IEntityTypeConfiguration<GameMediaRecord>
{
    public void Configure(EntityTypeBuilder<GameMediaRecord> e)
    {
        e.HasIndex(x => new { x.GameId, x.SortOrder });
    }
}
