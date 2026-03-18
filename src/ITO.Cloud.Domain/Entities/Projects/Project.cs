using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Entities.Inspections;

namespace ITO.Cloud.Domain.Entities.Projects;

public class Project : TenantEntity
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectType { get; set; } = "edificio";
    public string Status { get; set; } = "activo";
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EstimatedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public int? TotalUnits { get; set; }
    public Guid? ItoManagerId { get; set; }
    public string? MandanteName { get; set; }
    public string? MandanteContact { get; set; }
    public string? MandanteEmail { get; set; }
    public string? ConstructionPermit { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navegación
    public Company Company { get; set; } = null!;
    public ICollection<ProjectStage> Stages { get; set; } = [];
    public ICollection<ProjectSector> Sectors { get; set; } = [];
    public ICollection<ProjectUnit> Units { get; set; } = [];
    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<ProjectContractor> Contractors { get; set; } = [];
    public ICollection<Inspection> Inspections { get; set; } = [];
}
