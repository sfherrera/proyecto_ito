using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class ProjectContractor : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ContractorId { get; set; }
    public Guid? SpecialtyId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }

    public Project Project { get; set; } = null!;
    public Contractor Contractor { get; set; } = null!;
    public Specialty? Specialty { get; set; }
}
