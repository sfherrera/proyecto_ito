using ITO.Cloud.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Identity;

public class SequenceCounterConfiguration : IEntityTypeConfiguration<SequenceCounter>
{
    public void Configure(EntityTypeBuilder<SequenceCounter> builder)
    {
        builder.ToTable("sequence_counters");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Prefix).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => new { s.TenantId, s.EntityType, s.Year }).IsUnique();
    }
}
