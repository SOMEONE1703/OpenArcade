using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class ReportConfiguration : IEntityTypeConfiguration<ReportRecord>
{
    public void Configure(EntityTypeBuilder<ReportRecord> e)
    {
        e.HasIndex(x => new { x.Status, x.CreatedAt });
        e.Property(x => x.Reason).HasMaxLength(500);
    }
}
