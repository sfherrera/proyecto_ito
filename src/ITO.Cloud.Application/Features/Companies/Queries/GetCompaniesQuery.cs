using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Companies.DTOs;
using MediatR;

namespace ITO.Cloud.Application.Features.Companies.Queries;

public record GetCompaniesQuery(
    int Page = 1, int PageSize = 20,
    string? Search = null, bool? IsActive = null) : IRequest<PaginatedList<CompanyDto>>;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto>;
