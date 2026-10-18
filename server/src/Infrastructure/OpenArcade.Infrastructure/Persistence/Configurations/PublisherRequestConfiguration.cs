using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class PublisherRequestConfiguration : IEntityTypeConfiguration<PublisherRequestRecord>
{
    public void Configure(EntityTypeBuilder<PublisherRequestRecord> e)
    {
        e.HasIndex(x => new { x.UserId, x.Status });
    }
}
