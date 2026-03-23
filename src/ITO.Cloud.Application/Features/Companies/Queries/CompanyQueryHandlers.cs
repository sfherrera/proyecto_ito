using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Companies.DTOs;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Companies.Queries;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedList<CompanyDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetCompaniesQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<PaginatedList<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Companies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.Name.Contains(request.Search) || (c.Rut != null && c.Rut.Contains(request.Search)));

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedList<CompanyDto>.Create(_mapper.Map<List<CompanyDto>>(entities), total, request.Page, request.PageSize);
    }
}

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetCompanyByIdQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", request.Id);

        return _mapper.Map<CompanyDto>(company);
    }
}
