using ITO.Cloud.Domain.Entities.Projects;
using Microsoft.AspNetCore.Identity;

namespace ITO.Cloud.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }

    // Datos personales
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Rut { get; set; }
    public string? Position { get; set; }    // cargo: ITO, Supervisor, etc.
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Computed
    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navegación
    public Tenant Tenant { get; set; } = null!;
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
}
