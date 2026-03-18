using ITO.Cloud.Domain.Common;
using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Domain.Entities.Observations;

public class Observation : TenantEntity
{
    public Guid ProjectId { get; set; }
    public Guid? InspectionId { get; set; }
    public Guid? AnswerId { get; set; }
    public string Code { get; set; } = string.Empty;       // OBS-2026-0001
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SpecialtyId { get; set; }
    public string? Category { get; set; }                   // estructura, terminaciones, instalaciones...
    public ObservationSeverity Severity { get; set; } = ObservationSeverity.Media;
    public ObservationStatus Status { get; set; } = ObservationStatus.Abierta;
    public Guid? StageId { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? UnitId { get; set; }
    public string? LocationDescription { get; set; }
    public Guid? ContractorId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? AssignedById { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public Guid DetectedBy { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateOnly? ExtendedDueDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public int RejectionCount { get; set; } = 0;
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsRecurring { get; set; } = false;
    public bool IsOfflineCreated { get; set; } = false;
    public string? SyncId { get; set; }

    // Navegación
    public ICollection<ObservationHistory> History { get; set; } = [];
    public ICollection<InspectionEvidence> Evidence { get; set; } = [];
}
