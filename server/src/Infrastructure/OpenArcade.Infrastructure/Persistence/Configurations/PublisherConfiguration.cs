using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class PublisherConfiguration : IEntityTypeConfiguration<PublisherRecord>
{
    public void Configure(EntityTypeBuilder<PublisherRecord> e)
    {
        e.HasIndex(x => x.UserId).IsUnique();
    }
}
