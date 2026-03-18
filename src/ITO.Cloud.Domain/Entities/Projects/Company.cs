using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Projects;

public class Company : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? BusinessName { get; set; }          // razón social
    public string CompanyType { get; set; } = "constructora";
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navegación
    public ICollection<Project> Projects { get; set; } = [];
    public ICollection<Contractor> Contractors { get; set; } = [];
}
