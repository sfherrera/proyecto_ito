using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Observations.Commands;

public class CloseObservationCommandHandler : IRequestHandler<CloseObservationCommand, ObservationDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CloseObservationCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<ObservationDto> Handle(CloseObservationCommand request, CancellationToken cancellationToken)
    {
        var obs = await _db.Observations.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Observación", request.Id);

        var prev = obs.Status;
        obs.Status   = ObservationStatus.Cerrada;
        obs.ClosedAt = DateTime.UtcNow;
        obs.ClosedBy = _currentUser.UserId;

        _db.ObservationHistory.Add(new ObservationHistory
        {
            TenantId      = _currentUser.TenantId,
            ObservationId = obs.Id,
            Action        = "cerrada",
            PreviousStatus = prev.ToString(),
            NewStatus     = ObservationStatus.Cerrada.ToString(),
            Comment       = request.Comment,
            CreatedBy     = _currentUser.UserId
        });

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ObservationDto>(obs);
    }
}
