using ITO.Cloud.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Projects;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Rut).HasMaxLength(20);
        builder.Property(c => c.BusinessName).HasMaxLength(300);
        builder.Property(c => c.CompanyType).HasMaxLength(50).HasDefaultValue("constructora");
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Region).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Website).HasMaxLength(300);
        builder.Property(c => c.LogoUrl).HasMaxLength(500);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted });
    }
}
