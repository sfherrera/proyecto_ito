using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Inspections.Commands;

public class CancelInspectionCommandHandler : IRequestHandler<CancelInspectionCommand>
{
    private readonly IApplicationDbContext _db;
    public CancelInspectionCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CancelInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _db.Inspections.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Inspección", request.Id);

        if (inspection.Status == InspectionStatus.Cerrada)
            throw new ConflictException("No se puede cancelar una inspección ya cerrada.");

        inspection.Status = InspectionStatus.Cancelada;
        inspection.Notes  = string.IsNullOrEmpty(request.Reason) ? inspection.Notes : $"{inspection.Notes}\n[Cancelada]: {request.Reason}";
        await _db.SaveChangesAsync(cancellationToken);
    }
}
