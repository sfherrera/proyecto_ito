using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Entities.Identity;

namespace ITO.Cloud.Domain.Entities.Projects;

public class ProjectMember : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string ProjectRole { get; set; } = "inspector";  // director, supervisor, inspector, contratista, visualizador
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }

    // Navegación
    public Project Project { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
