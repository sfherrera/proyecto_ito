using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Templates;

public class TemplateVersion : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string Snapshot { get; set; } = "{}";  // JSON completo de la plantilla en ese momento
    public string? ChangeNotes { get; set; }
    public Guid CreatedBy { get; set; }

    public InspectionTemplate Template { get; set; } = null!;
}
