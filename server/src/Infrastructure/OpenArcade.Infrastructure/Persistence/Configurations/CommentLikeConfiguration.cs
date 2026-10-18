using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenArcade.Infrastructure.Persistence.Records;

namespace OpenArcade.Infrastructure.Persistence.Configurations;

internal sealed class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLikeRecord>
{
    public void Configure(EntityTypeBuilder<CommentLikeRecord> e)
    {
        e.HasKey(x => new { x.UserId, x.CommentId });
    }
}
