using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Models;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Observations.Queries;

public class GetObservationsQueryHandler : IRequestHandler<GetObservationsQuery, PaginatedList<ObservationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetObservationsQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<PaginatedList<ObservationDto>> Handle(GetObservationsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Observations.AsNoTracking();

        if (request.ProjectId.HasValue)    query = query.Where(o => o.ProjectId == request.ProjectId);
        if (request.Status.HasValue)       query = query.Where(o => o.Status == request.Status);
        if (request.Severity.HasValue)     query = query.Where(o => o.Severity == request.Severity);
        if (request.AssignedToId.HasValue) query = query.Where(o => o.AssignedToId == request.AssignedToId);
        if (request.ContractorId.HasValue) query = query.Where(o => o.ContractorId == request.ContractorId);
        if (request.Overdue == true)       query = query.Where(o => o.DueDate < DateOnly.FromDateTime(DateTime.UtcNow) && o.Status != ObservationStatus.Cerrada);

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(o => o.Severity)
            .ThenByDescending(o => o.DetectedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedList<ObservationDto>.Create(_mapper.Map<List<ObservationDto>>(entities), total, request.Page, request.PageSize);
    }
}

public class GetObservationByIdQueryHandler : IRequestHandler<GetObservationByIdQuery, ObservationDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetObservationByIdQueryHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<ObservationDto> Handle(GetObservationByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Observations.AsNoTracking();
        if (request.IncludeHistory) query = query.Include(o => o.History);

        var obs = await query.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Observación", request.Id);

        return _mapper.Map<ObservationDto>(obs);
    }
}

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _db;

    public GetDashboardQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var inspQuery = _db.Inspections.AsNoTracking();
        var obsQuery  = _db.Observations.AsNoTracking();

        if (request.ProjectId.HasValue)
        {
            inspQuery = inspQuery.Where(i => i.ProjectId == request.ProjectId);
            obsQuery  = obsQuery.Where(o => o.ProjectId == request.ProjectId);
        }

        var totalInspections      = await inspQuery.CountAsync(cancellationToken);
        var inspThisMonth         = await inspQuery.CountAsync(i => i.CreatedAt >= monthStart, cancellationToken);
        var openObs               = await obsQuery.CountAsync(o => o.Status != ObservationStatus.Cerrada, cancellationToken);
        var criticalObs           = await obsQuery.CountAsync(o => o.Severity == ObservationSeverity.Critica && o.Status != ObservationStatus.Cerrada, cancellationToken);
        var closedThisMonth       = await obsQuery.CountAsync(o => o.ClosedAt >= monthStart, cancellationToken);
        var overdueObs            = await obsQuery.CountAsync(o => o.DueDate < DateOnly.FromDateTime(now) && o.Status != ObservationStatus.Cerrada, cancellationToken);

        // Tiempo promedio de cierre (días)
        var closedWithDates = await obsQuery
            .Where(o => o.Status == ObservationStatus.Cerrada && o.ClosedAt.HasValue)
            .Select(o => new { o.DetectedAt, o.ClosedAt })
            .ToListAsync(cancellationToken);

        var avgDays = closedWithDates.Count > 0
            ? closedWithDates.Average(o => (o.ClosedAt!.Value - o.DetectedAt).TotalDays)
            : 0;

        var totalClosed = await obsQuery.CountAsync(o => o.Status == ObservationStatus.Cerrada, cancellationToken);
        var totalObs = await obsQuery.CountAsync(cancellationToken);
        var complianceRate = totalObs > 0 ? (double)totalClosed / totalObs * 100 : 0;

        // Inspecciones por semana (últimas 8)
        var byWeek = await inspQuery
            .Where(i => i.FinishedAt >= now.AddDays(-56))
            .GroupBy(i => i.FinishedAt!.Value.DayOfYear / 7)
            .Select(g => new { Week = g.Key, Count = g.Count() })
            .OrderBy(g => g.Week)
            .ToListAsync(cancellationToken);

        // Observaciones por severidad
        var bySeverity = await obsQuery
            .GroupBy(o => o.Severity)
            .Select(g => new { Severity = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Cumplimiento por contratista
        var contractorCompliance = await obsQuery
            .Where(o => o.ContractorId.HasValue)
            .GroupBy(o => o.ContractorId)
            .Select(g => new
            {
                ContractorId = g.Key,
                Total  = g.Count(),
                Closed = g.Count(o => o.Status == ObservationStatus.Cerrada)
            })
            .ToListAsync(cancellationToken);

        return new DashboardDto(
            TotalInspections:         totalInspections,
            InspectionsThisMonth:     inspThisMonth,
            OpenObservations:         openObs,
            CriticalObservations:     criticalObs,
            ClosedObservationsThisMonth: closedThisMonth,
            OverdueObservations:      overdueObs,
            AverageClosingDays:       Math.Round(avgDays, 1),
            ComplianceRate:           Math.Round(complianceRate, 1),
            InspectionsByWeek:        byWeek.Select(w => new ChartDataPoint($"S{w.Week % 8 + 1}", w.Count)).ToList(),
            ObservationsBySeverity:   bySeverity.Select(s => new ChartDataPoint(s.Severity, s.Count)).ToList(),
            ContractorCompliance:     contractorCompliance.Select(c => new ContractorComplianceDto(
                c.ContractorId.ToString()!, c.Total, c.Closed,
                c.Total > 0 ? Math.Round((double)c.Closed / c.Total * 100, 1) : 0)).ToList()
        );
    }
}
