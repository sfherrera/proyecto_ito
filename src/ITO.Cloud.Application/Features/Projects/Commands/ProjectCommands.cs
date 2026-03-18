using FluentValidation;
using ITO.Cloud.Application.Features.Projects.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Projects.Commands;

public record CreateProjectCommand(
    Guid CompanyId, string Code, string Name, string ProjectType,
    string? Description, string? Address, string? City, string? Region,
    DateOnly? StartDate, DateOnly? EstimatedEndDate, int? TotalUnits,
    Guid? ItoManagerId, string? MandanteName, string? MandanteEmail,
    string? ConstructionPermit, string? Notes) : IRequest<ProjectDto>;

public record UpdateProjectCommand(
    Guid Id, string Code, string Name, string ProjectType, string Status,
    string? Description, string? Address, string? City, string? Region,
    DateOnly? StartDate, DateOnly? EstimatedEndDate, int? TotalUnits,
    Guid? ItoManagerId, string? MandanteName, bool IsActive, string? Notes) : IRequest<ProjectDto>;

public record DeleteProjectCommand(Guid Id) : IRequest;

public record AddProjectMemberCommand(Guid ProjectId, Guid UserId, string ProjectRole) : IRequest;
public record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty().WithMessage("Empresa requerida.");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).WithMessage("Código requerido.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300).WithMessage("Nombre requerido.");
    }
}
