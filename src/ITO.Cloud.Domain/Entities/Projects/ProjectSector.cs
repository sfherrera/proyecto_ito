using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class ProjectSector : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? ParentSectorId { get; set; }  // jerarquía: Torre → Piso
    public string Name { get; set; } = string.Empty;
    public string SectorType { get; set; } = "sector";  // torre, bloque, sector, piso, area_comun
    public int OrderIndex { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navegación
    public Project Project { get; set; } = null!;
    public ProjectSector? ParentSector { get; set; }
    public ICollection<ProjectSector> ChildSectors { get; set; } = [];
    public ICollection<ProjectUnit> Units { get; set; } = [];
}
