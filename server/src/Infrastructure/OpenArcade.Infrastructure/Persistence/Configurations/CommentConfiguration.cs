using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<CommentRecord>
{
    public void Configure(EntityTypeBuilder<CommentRecord> e)
    {
        e.HasIndex(x => new { x.GameId, x.ParentId, x.CreatedAt });
        e.Property(x => x.Body).HasMaxLength(2000);
    }
}
