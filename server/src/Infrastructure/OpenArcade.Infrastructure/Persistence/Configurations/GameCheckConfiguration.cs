using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class GameCheckConfiguration : IEntityTypeConfiguration<GameCheckRecord>
{
    public void Configure(EntityTypeBuilder<GameCheckRecord> e)
    {
        e.HasIndex(x => new { x.GameId, x.Kind, x.CheckedAt });
    }
}
