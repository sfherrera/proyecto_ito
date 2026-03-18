using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class ProjectStage : TenantEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; } = 0;
    public string Status { get; set; } = "pendiente";
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public Project Project { get; set; } = null!;
}
