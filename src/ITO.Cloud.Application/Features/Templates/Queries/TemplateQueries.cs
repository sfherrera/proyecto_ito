using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Templates.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Templates.Queries;

public record GetTemplatesQuery(
    int Page = 1, int PageSize = 20,
    string? Search = null, string? Status = null,
    Guid? SpecialtyId = null) : IRequest<PaginatedList<TemplateDto>>;

public record GetTemplateByIdQuery(Guid Id, bool IncludeSections = true) : IRequest<TemplateDto>;
