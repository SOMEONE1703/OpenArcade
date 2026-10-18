using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteConfiguration : IEntityTypeConfiguration<FavoriteRecord>
{
    public void Configure(EntityTypeBuilder<FavoriteRecord> e)
    {
        e.HasKey(x => new { x.UserId, x.GameId });
    }
}
