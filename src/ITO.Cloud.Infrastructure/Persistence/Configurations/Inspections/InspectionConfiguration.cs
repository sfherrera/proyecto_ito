using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Inspections;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("inspections");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Code).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Title).HasMaxLength(300).IsRequired();
        builder.Property(i => i.InspectionType).HasMaxLength(100).HasDefaultValue("ordinaria");
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.WeatherConditions).HasMaxLength(100);
        builder.Property(i => i.SyncId).HasMaxLength(100);
        builder.Property(i => i.Score).HasPrecision(5, 2);
        builder.Property(i => i.PassingScore).HasPrecision(5, 2);
        builder.Property(i => i.Latitude).HasPrecision(10, 7);
        builder.Property(i => i.Longitude).HasPrecision(10, 7);
        builder.Property(i => i.Temperature).HasPrecision(4, 1);

        builder.HasIndex(i => new { i.TenantId, i.Code }).IsUnique()
            .HasFilter("is_deleted = false");
        builder.HasIndex(i => new { i.TenantId, i.Status });
        builder.HasIndex(i => new { i.TenantId, i.ScheduledDate });
        builder.HasIndex(i => i.AssignedToId);
        builder.HasIndex(i => i.SyncId).HasFilter("sync_id IS NOT NULL");

        builder.HasMany(i => i.Answers)
            .WithOne(a => a.Inspection)
            .HasForeignKey(a => a.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Evidence)
            .WithOne(e => e.Inspection)
            .HasForeignKey(e => e.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
