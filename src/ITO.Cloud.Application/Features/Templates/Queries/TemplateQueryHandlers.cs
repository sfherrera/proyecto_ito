using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Templates.Queries;

public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PaginatedList<TemplateDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetTemplatesQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<PaginatedList<TemplateDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.InspectionTemplates
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t => t.Name.Contains(request.Search) || (t.Description != null && t.Description.Contains(request.Search)));

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(t => t.Status.ToString() == request.Status);

        if (request.SpecialtyId.HasValue)
            query = query.Where(t => t.SpecialtyId == request.SpecialtyId.Value);

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedList<TemplateDto>.Create(_mapper.Map<List<TemplateDto>>(entities), total, request.Page, request.PageSize);
    }
}

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetTemplateByIdQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<TemplateDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.IncludeSections)
        {
            var template = await _db.InspectionTemplates
                .IgnoreAutoIncludes()
                .AsNoTracking()
                .Include(t => t.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Questions.OrderBy(q => q.OrderIndex))
                        .ThenInclude(q => q.Options.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("Plantilla", request.Id);

            return _mapper.Map<TemplateDto>(template);
        }
        else
        {
            var template = await _db.InspectionTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("Plantilla", request.Id);

            return _mapper.Map<TemplateDto>(template);
        }
    }
}
