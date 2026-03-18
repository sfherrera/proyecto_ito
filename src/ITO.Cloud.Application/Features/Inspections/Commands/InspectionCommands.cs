using FluentValidation;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Domain.Enums;
using MediatR;

namespace ITO.Cloud.Application.Features.Inspections.Commands;

public record CreateInspectionCommand(
    Guid ProjectId, Guid TemplateId, string Title,
    DateTime ScheduledDate, string InspectionType,
    InspectionPriority Priority,
    Guid? StageId, Guid? SectorId, Guid? UnitId,
    Guid? AssignedToId, Guid? ContractorId,
    Guid? SpecialtyId, string? Description,
    DateTime? ScheduledEndDate) : IRequest<InspectionDto>;

public record StartInspectionCommand(Guid Id) : IRequest<InspectionDto>;

public record SubmitInspectionCommand(
    Guid InspectionId,
    IList<AnswerSubmitDto> Answers,
    decimal? Latitude, decimal? Longitude,
    string? WeatherConditions, string? Notes) : IRequest<InspectionDto>;

public record CancelInspectionCommand(Guid Id, string? Reason) : IRequest;

public class CreateInspectionCommandValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ScheduledDate).GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("La fecha programada no puede ser en el pasado.");
    }
}
