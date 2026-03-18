using ITO.Cloud.Domain.Common;

namespace ITO.Cloud.Domain.Entities.Observations;

public class Reinspection : TenantEntity
{
    public Guid OriginalInspectionId { get; set; }
    public Guid? NewInspectionId { get; set; }
    public Guid? ObservationId { get; set; }
    public string Code { get; set; } = string.Empty;   // REI-2026-0001
    public string Status { get; set; } = "pendiente";  // pendiente, programada, ejecutada
    public string? Result { get; set; }                 // aprobada, rechazada
    public string? RejectionReason { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? ExecutedBy { get; set; }
}
