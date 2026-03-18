using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Identity;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;  // identificador URL: mi-empresa
    public string Plan { get; set; } = "basic";        // basic, professional, enterprise
    public bool IsActive { get; set; } = true;
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxProjects { get; set; } = 5;
    public new DateTime? UpdatedAt { get; set; }

    // Navegación
    public ICollection<ApplicationUser> Users { get; set; } = [];
}
