using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Application.Features.Inspections.DTOs;

public record InspectionDto(
    Guid Id, Guid ProjectId, Guid TemplateId, string Code, string Title,
    string InspectionType, InspectionStatus Status, InspectionPriority Priority,
    DateTime ScheduledDate, DateTime? StartedAt, DateTime? FinishedAt,
    Guid? AssignedToId, string? AssignedToName,
    Guid? ContractorId, string? ContractorName,
    Guid? StageId, Guid? SectorId, Guid? UnitId,
    decimal? Score, bool? Passed,
    int TotalQuestions, int AnsweredQuestions,
    int ConformingCount, int NonConformingCount,
    bool IsOfflineCreated, string? Notes, DateTime CreatedAt);

public record InspectionAnswerDto(
    Guid Id, Guid QuestionId, string? AnswerValue,
    bool? IsConforming, bool IsNa, decimal? Score, string? Notes);

public record InspectionEvidenceDto(
    Guid Id, Guid InspectionId, Guid? AnswerId,
    string FileType, string FileName, string FilePath,
    string? ThumbnailPath, string? Caption, DateTime CreatedAt);

public record CreateInspectionDto(
    Guid ProjectId, Guid TemplateId, string Title,
    DateTime ScheduledDate,
    string InspectionType = "ordinaria",
    InspectionPriority Priority = InspectionPriority.Normal,
    Guid? StageId = null, Guid? SectorId = null, Guid? UnitId = null,
    Guid? AssignedToId = null, Guid? ContractorId = null,
    Guid? SpecialtyId = null, string? Description = null,
    DateTime? ScheduledEndDate = null);

public record SubmitInspectionDto(
    Guid InspectionId,
    IList<AnswerSubmitDto> Answers,
    decimal? Latitude = null, decimal? Longitude = null,
    string? WeatherConditions = null, string? Notes = null);

public record AnswerSubmitDto(
    Guid QuestionId, string? AnswerValue,
    Guid? SelectedOptionId = null, decimal? NumericValue = null,
    DateOnly? DateValue = null, bool IsNa = false, string? Notes = null);
