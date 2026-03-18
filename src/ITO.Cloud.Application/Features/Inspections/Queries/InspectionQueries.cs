using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Domain.Enums;
using MediatR;

namespace ITO.Cloud.Application.Features.Inspections.Queries;

public record GetInspectionsQuery(
    int Page = 1, int PageSize = 20,
    Guid? ProjectId = null,
    Guid? AssignedToId = null,
    InspectionStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IRequest<PaginatedList<InspectionDto>>;

public record GetInspectionByIdQuery(Guid Id, bool IncludeAnswers = false) : IRequest<InspectionDto>;
public record GetMyInspectionsQuery(InspectionStatus? Status = null) : IRequest<IList<InspectionDto>>;
