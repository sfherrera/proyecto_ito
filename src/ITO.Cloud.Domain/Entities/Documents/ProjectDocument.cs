using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Entities.Projects;

namespace ITO.Cloud.Domain.Entities.Documents;

public class ProjectDocument : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? InspectionId { get; set; }
    public Guid? ObservationId { get; set; }
    public string Category { get; set; } = "general";  // plano, especificacion, contrato, procedimiento...
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public string? Version { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public Project Project { get; set; } = null!;
}
