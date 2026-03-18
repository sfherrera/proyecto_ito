namespace ITO.Cloud.Domain.Common;

/// <summary>
/// Entidad base para todas las entidades de negocio que pertenecen a un tenant.
/// EF Core aplica un Global Query Filter por TenantId automáticamente.
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    public Guid TenantId { get; set; }
}
