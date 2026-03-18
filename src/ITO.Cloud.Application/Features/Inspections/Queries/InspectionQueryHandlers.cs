using AutoMapper;
using AutoMapper.QueryableExtensions;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Inspections.Queries;

public class GetInspectionsQueryHandler : IRequestHandler<GetInspectionsQuery, PaginatedList<InspectionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetInspectionsQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<PaginatedList<InspectionDto>> Handle(GetInspectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Inspections.AsNoTracking();

        if (request.ProjectId.HasValue)     query = query.Where(i => i.ProjectId == request.ProjectId);
        if (request.AssignedToId.HasValue)  query = query.Where(i => i.AssignedToId == request.AssignedToId);
        if (request.Status.HasValue)        query = query.Where(i => i.Status == request.Status);
        if (request.FromDate.HasValue)      query = query.Where(i => i.ScheduledDate >= request.FromDate);
        if (request.ToDate.HasValue)        query = query.Where(i => i.ScheduledDate <= request.ToDate);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(i => i.ScheduledDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<InspectionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PaginatedList<InspectionDto>.Create(items, total, request.Page, request.PageSize);
    }
}

public class GetInspectionByIdQueryHandler : IRequestHandler<GetInspectionByIdQuery, InspectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetInspectionByIdQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<InspectionDto> Handle(GetInspectionByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Inspections.AsNoTracking();
        if (request.IncludeAnswers) query = query.Include(i => i.Answers).Include(i => i.Evidence);

        var inspection = await query.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Inspección", request.Id);

        return _mapper.Map<InspectionDto>(inspection);
    }
}

public class GetMyInspectionsQueryHandler : IRequestHandler<GetMyInspectionsQuery, IList<InspectionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public GetMyInspectionsQueryHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<IList<InspectionDto>> Handle(GetMyInspectionsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Inspections.AsNoTracking()
            .Where(i => i.AssignedToId == _currentUser.UserId);

        if (request.Status.HasValue) query = query.Where(i => i.Status == request.Status);

        return await query
            .OrderBy(i => i.ScheduledDate)
            .ProjectTo<InspectionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
