using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class ProjectUnit : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? SectorId { get; set; }
    public string UnitCode { get; set; } = string.Empty;   // 2B, 304, Casa 15
    public string UnitType { get; set; } = "departamento";
    public int? Floor { get; set; }
    public decimal? SurfaceM2 { get; set; }
    public string Status { get; set; } = "construccion";
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navegación
    public Project Project { get; set; } = null!;
    public ProjectSector? Sector { get; set; }
}
