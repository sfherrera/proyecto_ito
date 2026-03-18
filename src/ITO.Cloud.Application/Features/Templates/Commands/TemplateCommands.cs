using FluentValidation;
using ITO.Cloud.Application.Features.Templates.DTOs;
using ITO.Cloud.Domain.Enums;
using MediatR;

namespace ITO.Cloud.Application.Features.Templates.Commands;

public record CreateTemplateCommand(
    string Name, string? Description, string? TemplateType,
    Guid? SpecialtyId, bool IsGlobal, bool RequireGeolocation,
    bool RequireSignature, decimal? PassingScore) : IRequest<TemplateDto>;

public record UpdateTemplateCommand(
    Guid Id, string Name, string? Description,
    TemplateStatus Status, bool IsGlobal,
    bool RequireGeolocation, bool RequireSignature,
    decimal? PassingScore) : IRequest<TemplateDto>;

public record PublishTemplateCommand(Guid Id) : IRequest<TemplateDto>;
public record DeleteTemplateCommand(Guid Id) : IRequest;

public record AddTemplateSectionCommand(
    Guid TemplateId, string Title, string? Description,
    int OrderIndex, decimal Weight) : IRequest<TemplateSectionDto>;

public record AddTemplateQuestionCommand(
    Guid SectionId, string QuestionText, QuestionType QuestionType,
    int OrderIndex, bool IsRequired, bool IsCritical, decimal Weight,
    string? Description, Guid? ParentQuestionId, string? TriggerValue,
    decimal? MinValue, decimal? MaxValue, int MinPhotos, int MaxPhotos,
    IList<CreateOptionDto>? Options) : IRequest<TemplateQuestionDto>;

public record CreateOptionDto(string Label, string Value, int OrderIndex, bool IsFailureOption, decimal Score);

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
    }
}
