using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Observations.Commands;

public class CreateObservationCommandHandler : IRequestHandler<CreateObservationCommand, ObservationDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateObservationCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<ObservationDto> Handle(CreateObservationCommand request, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var counter = await _db.SequenceCounters
            .FirstOrDefaultAsync(s => s.EntityType == "observation" && s.Year == year, cancellationToken);

        if (counter == null)
        {
            counter = new SequenceCounter
            {
                TenantId = _currentUser.TenantId, EntityType = "observation",
                Prefix = "OBS", Year = year, LastValue = 1
            };
            _db.SequenceCounters.Add(counter);
        }
        else counter.LastValue++;

        var observation = new Observation
        {
            TenantId            = _currentUser.TenantId,
            ProjectId           = request.ProjectId,
            InspectionId        = request.InspectionId,
            AnswerId            = request.AnswerId,
            Code                = $"OBS-{year}-{counter.LastValue:D4}",
            Title               = request.Title,
            Description         = request.Description,
            Severity            = request.Severity,
            SpecialtyId         = request.SpecialtyId,
            Category            = request.Category,
            SectorId            = request.SectorId,
            UnitId              = request.UnitId,
            LocationDescription = request.LocationDescription,
            ContractorId        = request.ContractorId,
            AssignedToId        = request.AssignedToId,
            AssignedById        = request.AssignedToId.HasValue ? _currentUser.UserId : null,
            DueDate             = request.DueDate,
            RootCause           = request.RootCause,
            DetectedAt          = DateTime.UtcNow,
            DetectedBy          = _currentUser.UserId,
            CreatedBy           = _currentUser.UserId,
            Status              = request.AssignedToId.HasValue
                ? Domain.Enums.ObservationStatus.Asignada
                : Domain.Enums.ObservationStatus.Abierta
        };

        _db.Observations.Add(observation);

        // Registrar en historial
        _db.ObservationHistory.Add(new ObservationHistory
        {
            TenantId      = _currentUser.TenantId,
            ObservationId = observation.Id,
            Action        = "creada",
            NewStatus     = observation.Status.ToString(),
            CreatedBy     = _currentUser.UserId
        });

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ObservationDto>(observation);
    }
}

public class UpdateObservationStatusCommandHandler : IRequestHandler<UpdateObservationStatusCommand, ObservationDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UpdateObservationStatusCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<ObservationDto> Handle(UpdateObservationStatusCommand request, CancellationToken cancellationToken)
    {
        var obs = await _db.Observations
            .Include(o => o.History)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Observación", request.Id);

        var previousStatus = obs.Status;
        obs.Status = request.NewStatus;

        if (request.AssignedToId.HasValue)
        {
            obs.AssignedToId = request.AssignedToId;
            obs.AssignedById = _currentUser.UserId;
        }

        if (request.ExtendedDueDate.HasValue)
            obs.ExtendedDueDate = request.ExtendedDueDate;

        if (request.NewStatus == Domain.Enums.ObservationStatus.Cerrada)
        {
            obs.ClosedAt = DateTime.UtcNow;
            obs.ClosedBy = _currentUser.UserId;
        }

        if (request.NewStatus == Domain.Enums.ObservationStatus.Rechazada)
            obs.RejectionCount++;

        _db.ObservationHistory.Add(new ObservationHistory
        {
            TenantId             = _currentUser.TenantId,
            ObservationId        = obs.Id,
            Action               = "estado_cambiado",
            PreviousStatus       = previousStatus.ToString(),
            NewStatus            = request.NewStatus.ToString(),
            PreviousAssignedTo   = obs.AssignedToId,
            NewAssignedTo        = request.AssignedToId,
            Comment              = request.Comment,
            CreatedBy            = _currentUser.UserId
        });

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ObservationDto>(obs);
    }
}
