using ITO.Cloud.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Projects;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.ProjectType).HasMaxLength(50).HasDefaultValue("edificio");
        builder.Property(p => p.Status).HasMaxLength(50).HasDefaultValue("activo");
        builder.Property(p => p.Address).HasMaxLength(300);
        builder.Property(p => p.City).HasMaxLength(100);
        builder.Property(p => p.Region).HasMaxLength(100);
        builder.Property(p => p.Latitude).HasPrecision(10, 7);
        builder.Property(p => p.Longitude).HasPrecision(10, 7);
        builder.Property(p => p.MandanteName).HasMaxLength(200);
        builder.Property(p => p.MandanteContact).HasMaxLength(200);
        builder.Property(p => p.MandanteEmail).HasMaxLength(256);
        builder.Property(p => p.ConstructionPermit).HasMaxLength(100);

        builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique()
            .HasFilter("is_deleted = false");
        builder.HasIndex(p => new { p.TenantId, p.Status });

        builder.HasOne(p => p.Company)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
