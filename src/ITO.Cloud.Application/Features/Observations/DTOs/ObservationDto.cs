using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Application.Features.Observations.DTOs;

public class ObservationDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? InspectionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? SpecialtyId { get; set; }
    public string? Category { get; set; }
    public ObservationSeverity Severity { get; set; }
    public ObservationStatus Status { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? UnitId { get; set; }
    public string? LocationDescription { get; set; }
    public Guid? ContractorId { get; set; }
    public string? ContractorName { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int RejectionCount { get; set; }
    public bool IsRecurring { get; set; }
    public DateTime CreatedAt { get; set; }
    public IList<ObservationHistoryDto>? History { get; set; }
}

public record ObservationHistoryDto(
    Guid Id, string Action, string? PreviousStatus, string? NewStatus,
    string? Comment, Guid CreatedBy, DateTime CreatedAt);

public record CreateObservationDto(
    Guid ProjectId, string Title, string Description,
    ObservationSeverity Severity = ObservationSeverity.Media,
    Guid? InspectionId = null, Guid? AnswerId = null,
    Guid? SpecialtyId = null, string? Category = null,
    Guid? SectorId = null, Guid? UnitId = null,
    string? LocationDescription = null,
    Guid? ContractorId = null, Guid? AssignedToId = null,
    DateOnly? DueDate = null,
    string? RootCause = null, string? CorrectiveAction = null);

public record UpdateObservationStatusDto(
    ObservationStatus NewStatus, string? Comment = null,
    Guid? AssignedToId = null, DateOnly? ExtendedDueDate = null);

public record DashboardDto(
    int TotalInspections, int InspectionsThisMonth,
    int OpenObservations, int CriticalObservations,
    int ClosedObservationsThisMonth, int OverdueObservations,
    double AverageClosingDays, double ComplianceRate,
    IList<ChartDataPoint> InspectionsByWeek,
    IList<ChartDataPoint> ObservationsBySeverity,
    IList<ContractorComplianceDto> ContractorCompliance);

public record ChartDataPoint(string Label, int Value);
public record ContractorComplianceDto(string ContractorName, int Total, int Closed, double Rate);
