using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Projects.DTOs;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Projects.Queries;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PaginatedList<ProjectDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectsQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<PaginatedList<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Projects.Include(p => p.Company).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search) || p.Code.Contains(request.Search));

        if (request.CompanyId.HasValue)
            query = query.Where(p => p.CompanyId == request.CompanyId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(p => p.Status == request.Status);

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedList<ProjectDto>.Create(_mapper.Map<List<ProjectDto>>(entities), total, request.Page, request.PageSize);
    }
}

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.Include(p => p.Company).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Proyecto", request.Id);

        return _mapper.Map<ProjectDto>(project);
    }
}

public class GetProjectStagesQueryHandler : IRequestHandler<GetProjectStagesQuery, IList<ProjectStageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectStagesQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<IList<ProjectStageDto>> Handle(GetProjectStagesQuery request, CancellationToken cancellationToken)
    {
        var stages = await _db.ProjectStages.AsNoTracking()
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<ProjectStageDto>>(stages);
    }
}

public class GetProjectSectorsQueryHandler : IRequestHandler<GetProjectSectorsQuery, IList<ProjectSectorDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectSectorsQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<IList<ProjectSectorDto>> Handle(GetProjectSectorsQuery request, CancellationToken cancellationToken)
    {
        var sectors = await _db.ProjectSectors.AsNoTracking()
            .Where(s => s.ProjectId == request.ProjectId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<ProjectSectorDto>>(sectors);
    }
}

public class GetProjectUnitsQueryHandler : IRequestHandler<GetProjectUnitsQuery, IList<ProjectUnitDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProjectUnitsQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<IList<ProjectUnitDto>> Handle(GetProjectUnitsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.ProjectUnits.AsNoTracking()
            .Where(u => u.ProjectId == request.ProjectId);

        if (request.SectorId.HasValue)
            query = query.Where(u => u.SectorId == request.SectorId.Value);

        var units = await query
            .OrderBy(u => u.UnitCode)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<ProjectUnitDto>>(units);
    }
}
