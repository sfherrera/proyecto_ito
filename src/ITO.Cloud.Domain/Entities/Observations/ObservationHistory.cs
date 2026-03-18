using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Observations;

public class ObservationHistory : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ObservationId { get; set; }
    public string Action { get; set; } = string.Empty;  // creada, asignada, estado_cambiado, comentario, cerrada
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public Guid? PreviousAssignedTo { get; set; }
    public Guid? NewAssignedTo { get; set; }
    public string? Comment { get; set; }
    public Guid CreatedBy { get; set; }

    public Observation Observation { get; set; } = null!;
}
