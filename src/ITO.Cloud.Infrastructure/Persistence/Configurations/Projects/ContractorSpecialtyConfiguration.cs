using ITO.Cloud.Domain.Entities.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Projects;

public class ContractorSpecialtyConfiguration : IEntityTypeConfiguration<ContractorSpecialty>
{
    public void Configure(EntityTypeBuilder<ContractorSpecialty> builder)
    {
        builder.HasKey(cs => new { cs.ContractorId, cs.SpecialtyId });

        builder.ToTable("contractor_specialties");

        builder.HasOne(cs => cs.Contractor)
               .WithMany(c => c.Specialties)
               .HasForeignKey(cs => cs.ContractorId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Specialty)
               .WithMany()
               .HasForeignKey(cs => cs.SpecialtyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
