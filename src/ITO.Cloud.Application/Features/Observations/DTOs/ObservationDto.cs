using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Application.Features.Observations.DTOs;

public record ObservationDto(
    Guid Id, Guid ProjectId, Guid? InspectionId,
    string Code, string Title, string Description,
    Guid? SpecialtyId, string? Category,
    ObservationSeverity Severity, ObservationStatus Status,
    Guid? SectorId, Guid? UnitId, string? LocationDescription,
    Guid? ContractorId, string? ContractorName,
    Guid? AssignedToId, string? AssignedToName,
    DateTime DetectedAt, DateOnly? DueDate,
    DateTime? ClosedAt, int RejectionCount,
    bool IsRecurring, DateTime CreatedAt,
    IList<ObservationHistoryDto>? History = null);

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
