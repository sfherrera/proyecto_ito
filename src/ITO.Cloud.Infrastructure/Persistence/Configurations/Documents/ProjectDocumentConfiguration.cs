using ITO.Cloud.Domain.Entities.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITO.Cloud.Infrastructure.Persistence.Configurations.Documents;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("project_documents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(256).IsRequired();
        builder.Property(e => e.FileName).HasMaxLength(512).IsRequired();
        builder.Property(e => e.FilePath).HasMaxLength(1024).IsRequired();
        builder.Property(e => e.Category).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1024);
        builder.Property(e => e.MimeType).HasMaxLength(128);
        builder.Property(e => e.Version).HasMaxLength(32);

        builder.HasIndex(e => new { e.TenantId, e.ProjectId });
        builder.HasIndex(e => e.Category);
    }
}
