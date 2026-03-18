using FluentValidation;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Domain.Enums;
using MediatR;

namespace ITO.Cloud.Application.Features.Observations.Commands;

public record CreateObservationCommand(
    Guid ProjectId, string Title, string Description,
    ObservationSeverity Severity,
    Guid? InspectionId, Guid? AnswerId,
    Guid? SpecialtyId, string? Category,
    Guid? SectorId, Guid? UnitId, string? LocationDescription,
    Guid? ContractorId, Guid? AssignedToId,
    DateOnly? DueDate, string? RootCause) : IRequest<ObservationDto>;

public record UpdateObservationStatusCommand(
    Guid Id, ObservationStatus NewStatus,
    string? Comment, Guid? AssignedToId,
    DateOnly? ExtendedDueDate) : IRequest<ObservationDto>;

public record CloseObservationCommand(Guid Id, string? Comment) : IRequest<ObservationDto>;

public class CreateObservationCommandValidator : AbstractValidator<CreateObservationCommand>
{
    public CreateObservationCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty();
    }
}
