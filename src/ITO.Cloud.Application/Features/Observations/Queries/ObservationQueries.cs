using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Domain.Enums;
using MediatR;

namespace ITO.Cloud.Application.Features.Observations.Queries;

public record GetObservationsQuery(
    int Page = 1, int PageSize = 20,
    Guid? ProjectId = null,
    ObservationStatus? Status = null,
    ObservationSeverity? Severity = null,
    Guid? AssignedToId = null,
    Guid? ContractorId = null,
    bool? Overdue = null) : IRequest<PaginatedList<ObservationDto>>;

public record GetObservationByIdQuery(Guid Id, bool IncludeHistory = true) : IRequest<ObservationDto>;

public record GetDashboardQuery(Guid? ProjectId = null) : IRequest<DashboardDto>;
