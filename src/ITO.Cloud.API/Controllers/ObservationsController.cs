using ITO.Cloud.Application.Features.Observations.Commands;
using ITO.Cloud.Application.Features.Observations.DTOs;
using ITO.Cloud.Application.Features.Observations.Queries;
using ITO.Cloud.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ITO.Cloud.API.Controllers;

/// <summary>Gestión de observaciones y no conformidades</summary>
public class ObservationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? projectId = null,
        [FromQuery] ObservationStatus? status = null,
        [FromQuery] ObservationSeverity? severity = null,
        [FromQuery] Guid? assignedToId = null,
        [FromQuery] Guid? contractorId = null,
        [FromQuery] bool? overdue = null) =>
        OkPaginated(await Mediator.Send(new GetObservationsQuery(
            page, pageSize, projectId, status, severity, assignedToId, contractorId, overdue)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeHistory = true) =>
        OkData(await Mediator.Send(new GetObservationByIdQuery(id, includeHistory)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateObservationDto dto) =>
        CreatedData(await Mediator.Send(new CreateObservationCommand(
            dto.ProjectId, dto.Title, dto.Description, dto.Severity,
            dto.InspectionId, dto.AnswerId, dto.SpecialtyId, dto.Category,
            dto.SectorId, dto.UnitId, dto.LocationDescription,
            dto.ContractorId, dto.AssignedToId, dto.DueDate, dto.RootCause)));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateObservationStatusDto dto) =>
        OkData(await Mediator.Send(new UpdateObservationStatusCommand(
            id, dto.NewStatus, dto.Comment, dto.AssignedToId, dto.ExtendedDueDate)));

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromQuery] string? comment = null) =>
        OkData(await Mediator.Send(new CloseObservationCommand(id, comment)));
}
