using ITO.Cloud.Domain.Enums;

namespace ITO.Cloud.Application.Features.Templates.DTOs;

public record TemplateDto(
    Guid Id, string Name, string? Description, string? TemplateType,
    Guid? SpecialtyId, TemplateStatus Status, int CurrentVersion,
    bool IsGlobal, bool RequireGeolocation, bool RequireSignature,
    decimal? PassingScore, DateTime CreatedAt,
    IList<TemplateSectionDto>? Sections = null);

public record TemplateSectionDto(
    Guid Id, Guid TemplateId, string Title, string? Description,
    int OrderIndex, bool IsRequired, decimal Weight,
    IList<TemplateQuestionDto>? Questions = null);

public record TemplateQuestionDto(
    Guid Id, Guid SectionId, Guid? ParentQuestionId, string? TriggerValue,
    string QuestionText, string? Description, QuestionType QuestionType,
    int OrderIndex, bool IsRequired, bool IsCritical, decimal Weight,
    decimal? MinValue, decimal? MaxValue, int MinPhotos, int MaxPhotos,
    IList<TemplateQuestionOptionDto>? Options = null);

public record TemplateQuestionOptionDto(
    Guid Id, string Label, string Value, int OrderIndex,
    bool IsFailureOption, decimal Score);

public record CreateTemplateDto(
    string Name, string? Description, string? TemplateType,
    Guid? SpecialtyId = null, bool IsGlobal = false,
    bool RequireGeolocation = false, bool RequireSignature = false,
    decimal? PassingScore = null);

public record CreateTemplateSectionDto(
    string Title, string? Description = null,
    int OrderIndex = 0, decimal Weight = 1.0m);

public record CreateTemplateQuestionDto(
    Guid SectionId, string QuestionText, QuestionType QuestionType,
    int OrderIndex = 0, bool IsRequired = true, bool IsCritical = false,
    decimal Weight = 1.0m, string? Description = null,
    Guid? ParentQuestionId = null, string? TriggerValue = null,
    decimal? MinValue = null, decimal? MaxValue = null,
    int MinPhotos = 0, int MaxPhotos = 10);
