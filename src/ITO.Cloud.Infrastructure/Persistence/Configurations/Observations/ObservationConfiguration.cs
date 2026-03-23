using ITO.Cloud.Domain.Entities.Observations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Observations;

public class ObservationConfiguration : IEntityTypeConfiguration<Observation>
{
    public void Configure(EntityTypeBuilder<Observation> builder)
    {
        builder.ToTable("observations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Code).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Title).HasMaxLength(300).IsRequired();
        builder.Property(o => o.Description).IsRequired();
        builder.Property(o => o.Category).HasMaxLength(100);
        builder.Property(o => o.Severity);
        builder.Property(o => o.Status);
        builder.Property(o => o.LocationDescription).HasMaxLength(300);
        builder.Property(o => o.SyncId).HasMaxLength(100);
        builder.Property(o => o.Latitude).HasPrecision(10, 7);
        builder.Property(o => o.Longitude).HasPrecision(10, 7);

        builder.HasIndex(o => new { o.TenantId, o.Code }).IsUnique()
            .HasFilter("is_deleted = false");
        builder.HasIndex(o => new { o.TenantId, o.Status });
        builder.HasIndex(o => new { o.TenantId, o.Severity });
        builder.HasIndex(o => o.AssignedToId);
        builder.HasIndex(o => new { o.TenantId, o.DueDate });

        builder.HasMany(o => o.History)
            .WithOne(h => h.Observation)
            .HasForeignKey(h => h.ObservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
