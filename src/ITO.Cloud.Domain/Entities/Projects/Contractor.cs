using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class Contractor : TenantEntity
{
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navegación
    public Company? Company { get; set; }
    public ICollection<ContractorSpecialty> Specialties { get; set; } = [];
}
