using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Projects.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Projects.Queries;

public record GetProjectsQuery(
    int Page = 1, int PageSize = 20,
    string? Search = null, Guid? CompanyId = null,
    string? Status = null) : IRequest<PaginatedList<ProjectDto>>;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;
public record GetProjectStagesQuery(Guid ProjectId) : IRequest<IList<ProjectStageDto>>;
public record GetProjectSectorsQuery(Guid ProjectId) : IRequest<IList<ProjectSectorDto>>;
public record GetProjectUnitsQuery(Guid ProjectId, Guid? SectorId = null) : IRequest<IList<ProjectUnitDto>>;
